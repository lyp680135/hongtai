<template>
  <div>
        <mt-field label="收货单位名称" placeholder="输入单位名称" v-model="companyname"></mt-field>
        <div class="ReceivUnitTip">
                小提示：收货单位不输则打印空白名称
        </div>
        <div class="list">
            <mt-cell v-for="(item,index) in list" :key="index">
                <div slot="title" class="iteminfo">
                    <div>
                      <p>{{item.Name}}</p>
                      <p>{{item.MaterialName}}/{{item.Specname}}</p>
                      <p>授{{item.AuthNumber}}/剩{{item.Number-item.AuthNumber}}件</p>
                    </div>
                    <div class="itemcode">
                      {{item.Batcode}}
                    </div>
                </div>
                <div>
                    <mt-button size="small" class="calcbtn" type="default"  @click.native="minus(index)">-</mt-button>  
                    <input class="number"  type="number" v-model="item.SelectedNumber" @blur="calc(index)"/>
                    <mt-button size="small" class="calcbtn"  type="default" @click.native="plus(index)">+</mt-button>
                    件
                </div>
            </mt-cell>
        </div>
        <div class="bottom_css">
            <div class="botton">
                <mt-button type="primary" @click="preview()">模版预览</mt-button>
            </div>
        </div>
  </div>
</template>

<script>
import xyHelper from '../plugins/xyHelper'

export default {
  name: 'TemplatPreView',
  data () {
    return {
      list: [],
      companyname: ''
    }
  },
  methods: {
    preview: function () {
      var vue = this
      if (this.list.length === 0) {
        this.$toast({message: '产品物资为空，请重新选择后再进行预览', position: 'bottom'})
        return
      }
      var requestData = {
        companyname: vue.companyname,
        list: this.list
      }
      this.$http.post('api/v1/Warranty/?requestData=' + JSON.stringify(requestData)).then(function (res) {
        if (typeof res === 'number' && res.toString().length === 8) {
          localStorage.removeItem('GenWarrantylist')
          // 生成质保书
          //  this.$router.push('/WarrantyBook/' + item.PrintNo + '/1')
          vue.$router.push('/WarrantyBook/' + res + '')
        } else {
          vue.$toast({message: res.data, position: 'bottom'})
        }
      }).catch(function (error) {
        if (error) {
          vue.$toast({message: '数据请求失败！', position: 'bottom'})
        }
      })
    },
    minus: function (index) {
      if (this.list[index].SelectedNumber > 0) {
        this.list[index].SelectedNumber = parseInt(this.list[index].SelectedNumber) - 1
      }
    },
    calc: function (index) {
      try {
        this.list[index].SelectedNumber = parseInt(this.list[index].SelectedNumber)
        var num = this.list[index].Number - this.list[index].AuthNumber
        if (this.list[index].SelectedNumber > num) {
          this.$toast({message: '授权件数不能大于剩余件数', position: 'bottom'})
          this.list[index].SelectedNumber = num
        }
      } catch (e) {
        this.list[index].SelectedNumber = 0
      }
    },
    plus: function (index) {
      var num = this.list[index].Number - this.list[index].AuthNumber
      if (this.list[index].SelectedNumber < num) {
        this.list[index].SelectedNumber = parseInt(this.list[index].SelectedNumber) + 1
      }
    }
  },
  mounted: function () {
    var data = xyHelper.tryParseJson(localStorage.GenWarrantylist)
    if (data) {
      this.list = data
    }
  }
}
</script>
<!-- Add "scoped" attribute to limit CSS to this component only -->
<style>
.mint-field .mint-cell-title{
    width:96px;
}
.mint-cell-title{
    padding:5px 0px;
}
</style>
<style scoped>
.ReceivUnitTip{
    padding-left:10px;
    color:red;
}

.list{
    margin-top:13px;
    font-size:12px;
}

.list p{
    padding:2px 0px;
    margin:0px;
    font-size:12px;
}

.iteminfo{
    display: flex;
    justify-content: space-between;
    align-items: center;
}
.itemcode{
  flex:1px;text-align:center;
}
.number{
    width: 40px;
    border: 0;
    box-shadow: 0 0 1px #7a7a7b;
    height: 30px;
    border-radius: 3px;
    text-align: center;
    font-size:13px;
}
.bottom_css{
    padding:15px;
}
.bottom_css button{
    width:100%;
}
</style>
