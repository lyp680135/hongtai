<template>
  <div class="set">
    <div class="title">
        <div class="col1">规格</div>
        <div class="col2">本月授权</div>
        <div class="col3">剩余</div>
        <div class="col4">增减</div>
    </div>
    <div v-for="(item,index) in workShopData.Data" :key="index">
      <div class="pm" @click="isShowData(index)">
          {{item.ClassName}}/{{item.MaterialName}}
          <i v-if="expandFlagList[index].flag" class="flagPic"></i>
          <i v-else class="flagPicFalse"></i>
      </div>
      <div class="data" v-show="expandFlagList[index].flag" v-for="(itemSpec,indexSpec) in item.Spec" :key="indexSpec">
          <div class="col1">{{itemSpec.SpecName}}</div>
          <div class="col2">{{itemSpec.Number_Auth}}</div>
          <div class="col3">{{itemSpec.Number_Available}}</div>
          <div class="col4">
            <mt-button size="small" type="default"  @click.native="reduce(index,indexSpec)">-</mt-button>
              <input type="number" class="datafiled" v-model.number="itemSpec.EditNumber"  />
            <mt-button size="small" type="default" @click.native="add(index,indexSpec)">+</mt-button>
          </div>
      </div>
    </div>
    <div class="line"></div>
    <div class="foot">
        <span class="footleft">{{editCount < 0 ? '减少':'新增'}}<b>{{editCount < 0 ? Math.abs(editCount) : editCount}}</b>件</span>
        <span class="footright"> <mt-button size="small" type="primary" :disabled='isDisabledButton'  @click.native="submit()">确认</mt-button></span>
    </div>
  </div>
</template>

<script>

export default {
  name: 'SetQrCodeNumData',
  data () {
    return {
      id: 0,
      title: '车间二维码授权',
      workShopData: [],
      expandFlagList: []
    }
  },
  computed: {
    editCount: function () {
      let ec = 0
      if (this.workShopData.Data !== undefined) {
        for (var item of this.workShopData.Data) {
          for (var itemDataSpec of item.Spec) {
            ec = itemDataSpec.EditNumber + ec
          }
        }
      }
      return ec
    },
    isDisabledButton: function () {
      let flag = true
      if (this.workShopData.Data !== undefined) {
        for (var item of this.workShopData.Data) {
          for (var itemDataSpec of item.Spec) {
            if (itemDataSpec.EditNumber !== 0) {
              flag = false
            }
          }
        }
      }
      return flag
    }
  },
  mounted () {
    let vue = this
    vue.id = this.$route.params.id

    this.$http.get('api/v1/WorkShop/' + vue.id).then(res => {
      for (var index in res.Data) {
        var expandFlag = {number: index, flag: true}
        vue.expandFlagList.push(expandFlag)
      }
      vue.workShopData = res
      vue.title = res.Name + ' - 车间二维码授权'
      this.$emit('viewLoaded', this)
    }).catch(e => {
    })
  },
  watch: {
    workShopData: {
      handler: function (val) {
        if (this.workShopData.Data !== undefined) {
          for (var item of this.workShopData.Data) {
            for (var valueSpec of item.Spec) {
              if (valueSpec.EditNumber < 0 && Math.abs(valueSpec.EditNumber) > valueSpec.Number_Auth) {
                valueSpec.EditNumber = -valueSpec.Number_Auth
              }
            }
          }
        }
      },
      deep: true
    }
  },
  methods: {
    isShowData: function (index) {
      this.expandFlagList[index].flag = !this.expandFlagList[index].flag
    },
    add: function (index, indexSpec) {
      this.workShopData.Data[index].Spec[indexSpec].EditNumber++
    },
    reduce: function (index, indexSpec) {
      this.workShopData.Data[index].Spec[indexSpec].EditNumber--
    },
    submit: function () {
      let vue = this
      vue.$indicator.open()

      this.$http.put('api/v1/WorkShop/' + vue.id, vue.workShopData).then(res => {
        vue.workShopData = res
        vue.$indicator.close()
        vue.$toast({
          message: '操作成功',
          iconClass: 'icon iconfont icon-success'
        })
      }).catch(e => {
        if (e !== undefined) {
          vue.$indicator.close()
          vue.$toast({
            message: e,
            position: 'bottom'
          })
        }
      })
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.set .data .mint-button--small{
  padding: 0;
  width: 25px;
}
h1, h2 {
  font-weight: normal;
}
a{
    text-decoration: none;
    color: #0066cc;
}
.set{
 width: 100%;
}
.set .title
{
    width: 100%;
    text-align: center;
    height: 35px;
    font-size: 14px;
    line-height: 35px;
}
.set .col1
{
  width: 30%;
}
.set .col1
{
  width: 20%;
  display: inline-block;
}
.set .col2
{
  width: 20%;
  display: inline-block;
}
.set .col3
{
  width: 20%;
  display: inline-block;
}
.set .col4
{
  width: 36%;
  display: inline-block;
}
.set .pm{
    height: 35px;
    background-color: #ccc;
    font-weight: 600;
    line-height: 35px;
    font-size: 14px;
    margin: 3px;
    padding-left: 10px;
    border-radius:3px;
}
.set .pm .flagPic{
    float: right;
    margin: 13px 13px 0 0;
    border: solid 1px #8b8b8c;
    border-bottom-width: 0;
    border-left-width: 0;
    content: " ";
    width: 10px;
    height: 10px;
    -webkit-transform: translateY(-50%) rotate(135deg);
    transform: translateY(-50%) rotate(135deg);
}
.set .pm .flagPicFalse{
       float: right;
    margin: 20px 13px 0px 0;
    border: solid 1px #8b8b8c;
    border-top-width: 0;
    border-right-width: 0;
    content: " ";
    width: 10px;
    height: 10px;
    -webkit-transform: translateY(-50%) rotate(135deg);
    transform: translateY(-50%) rotate(135deg);
}
.set .data{
  height: 40px;
    font-size: 14px;
    line-height: 40px;
    text-align: center;
    background-image: -webkit-gradient(linear, left top, left bottom, from(#d9d9d9), color-stop(50%, #d9d9d9), color-stop(50%, transparent));
    background-image: linear-gradient(180deg, #d9d9d9, #d9d9d9 50%, transparent 50%);
    background-size: 120% 1px;
    background-repeat: no-repeat;
    background-position: bottom left;
    background-origin: content-box;
    -webkit-box-align: center;
    -ms-flex-align: center;
    align-items: center;
    -webkit-box-sizing: border-box;
    box-sizing: border-box;
}
.set .data .datafiled{
    width: 40px;
    border: 0;
    box-shadow: 0 0 1px #7a7a7b;
    height: 30px;
    border-radius: 3px;
    text-align: center;
    font-size:13px;
}
.set .foot{
  position: fixed;
  width: 100%;
  bottom: 0;
  height: 45px;
  line-height: 45px;
  background-color: #f2f2f2;
}
.set .foot .footleft{
  font-size: 14px;
    margin-left: 20px;
    color: #646464;
}
.set .foot .footleft b{
  color: #F60;
  margin: 0 3px 0 3px;
}
.set .foot .footright{
  float: right;
  margin: 2px 15px 0 0;
}
.line{
   height: 55px;
    border: none;
    width: 100%;

}
</style>
