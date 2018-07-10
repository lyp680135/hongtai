<template>
  <div class="stockout">
    <zui-lisenseplate class="mycellcontainer" v-model="lisenseplate" label="车号" placeholder="请输入车牌">
      <i></i>
    </zui-lisenseplate>
    <mt-field class="mycell" label="售达方" placeholder="请选择售达方" readonly="readonly" v-model="saler.name" @click.native="showSalerList('/SelectTopSaler')">
      <i class="mint-cell-allow-right"></i>
    </mt-field>
    
    <div class="v-seperator"></div>
    <div class="container tips">
      提示：请按对应炉号的捆号由大到小顺序发货
    </div>
    <div class="container list">
      <div class="listitem" v-for="(item,index) in list" v-bind:key="item.batcode">
        <div class="listleft" @click="selectBatcode(index)">
          <i class="iconfont icon-add" v-if="(item.batcode == null)"></i>
          <div class="batcode" v-else>{{item.batcode}}</div>
          <div>轧制批号
          </div>
        </div>
        <div class="listright">
          从 <input type="tel" size="5" v-model="item.startbundle" :disabled="item.material==null" @click="showBundleCode(item,index,'start')"/>捆至
          <input type="tel" size="5" v-model="item.endbundle" :disabled="item.material==null" @click="showBundleCode(item,index,'end')"/> 捆
        </div>
        <div class="count" v-if="item.material!=null">{{(item.material == null)?'--':item.material}}/{{(item.spec == null)?'--':item.spec}}<label>{{(item.quantity == null)?'--':item.quantity}}件/{{(item.weight == null)?'--':item.weight}}吨</label></div>
        <div class="count" v-else>&nbsp;</div>
        <div class="h-line clear"></div>
      </div>
      
      <div class="listitem" @click="addnew()">
        <i class="iconfont icon-add"></i>
        增加行
      </div>
    </div>
    <div class="container" style="margin:10px 0">
      <mt-button type="primary" size="large" @click.native="showSalerList('/NoRuleStockOutput')" style="margin:5px 0">非尺产品出库</mt-button>
      <mt-button type="primary" size="large" @click="sb">确认</mt-button>
    </div>

    <mt-popup
      v-model="selectBatcodeFlag"
      position="bottom">
      <mt-picker :slots="slots" @change="onBatcodeSelected" value-key="name" show-toolbar>
        <span class="mint-datetime-action mint-datetime-cancel" @click="cancel()" v-cloak>取消</span>
        <span class="mint-datetime-action mint-datetime-confirm" @click="confirm()" v-cloak>确定</span>
      </mt-picker>
    </mt-popup>
  </div>
</template>
<script>
import xyHelper from '../plugins/xyHelper'
import Bus from '../event/bus'

function loadBatcodes (vue) {
  vue.$http.get('api/v1/Product', {
    showloading: true
  }).then(function (res) {
    var datalist = []

    if (xyHelper.isNullOrEmpty(res)) {

    } else {
      datalist = xyHelper.tryParseJson(res)
    }

    if (datalist.status === 0) {
      vue.$toast({ message: datalist.msg, position: 'middle' })
      return
    }

    vue.batcodes.push.apply(vue.batcodes, datalist)

    for (var i = 0; i < vue.batcodes.length; i++) {
      var module = {
        name: vue.batcodes[i].Batcode,
        id: vue.batcodes[i].Id
      }
      vue.slots[0].values.push(module)
    }
  })
}

function loadProductInfo (vue, batcode, index) {
  vue.$http.get('api/v1/Product/' + batcode, {
    showloading: true
  }).then(function (res) {
    var data = null
    if (xyHelper.isNullOrEmpty(res)) {

    } else {
      data = xyHelper.tryParseJson(res)
    }

    if (!xyHelper.isNullOrEmpty(data)) {
      if (data.status === 0) {
        vue.$toast({message: data.msg, position: 'bottom'})
      } else {
        vue.list[index].material = data.materialname
        vue.list[index].spec = data.specname
        vue.list[index].pieceweight = data.pieceweight
      }
    } else {
      vue.$toast({ message: '该批号下没有产品记录', position: 'bottom' })
    }
  })
}

export default {
  name: 'StockOutput',
  data () {
    return {
      title: '产品出库',
      selectBatcodeFlag: false,
      selectedIndex: null,
      pickerSelected: null,
      lisenseplate: '',
      saler: '',
      batcodes: [],
      list: [
        {
          'batcode': null,
          'startbundle': null,
          'endbundle': null,
          'material': null,
          'spec': null,
          'quantity': null,
          'weight': null,
          'pieceweight': null
        }
      ],
      slots: [
        {
          flex: 1,
          values: [{'name': '--', 'Id': null}],
          className: 'slot',
          textAlign: 'center'
        }
      ]
    }
  },
  mounted () {
    Bus.$on('topsalerSelected', function (val) {
      if (val !== undefined) {
        this.saler = val
      }
    }.bind(this))

    Bus.$on('bundlecodeselected', function (val) {
      if (val !== undefined) {
        var i = val.index
        if (val.deliverytype === 0) {
          if (val.type === 'start') {
            this.list[i].startbundle = val.bundlecode
          } else {
            this.list[i].endbundle = val.bundlecode
          }
        }
      }
    }.bind(this))
    this.$emit('viewLoaded', this)
  },
  methods: {
    showSalerList: function (redirectUrl) {
      this.$router.push(redirectUrl)
    },
    showBundleCode: function (obj, index, type) {
      this.$router.push('/SelectBundleCode/' + type + '/' + obj.batcode + '/' + index + '/0')
    },
    addnew: function () {
      this.list.push({
        'batcode': null,
        'startbundle': null,
        'endbundle': null,
        'material': null,
        'spec': null,
        'quantity': null,
        'weight': null,
        'pieceweight': null
      })
    },
    selectBatcode (index) {
      this.selectedIndex = index

      // 判断批号列表是否已经下载下来
      if (xyHelper.isNullOrEmpty(this.batcodes)) {
        loadBatcodes(this)
      }

      this.selectBatcodeFlag = true
    },
    onBatcodeSelected (picker, values) {
      if (Object.prototype.toString.call(values) === '[object Array]') {
        if (values[0] !== undefined) {
          if (values[0].name === '--') {

          }

          this.pickerSelected = values[0]
        }
      }
    },
    cancel () {
      this.selectBatcodeFlag = false
    },
    confirm () {
      this.selectBatcodeFlag = false
      if (this.selectedIndex >= 0 && this.pickerSelected !== null) {
        this.list[this.selectedIndex].batcode = this.pickerSelected.name
        // 自动加载该轧制批号的产品信息
        loadProductInfo(this, this.pickerSelected.name, this.selectedIndex)
      }
    },
    sb () {
      var vue = this
      var err = null
      if (xyHelper.isNullOrEmpty(this.lisenseplate)) {
        err = '请输入车牌号'
      }

      if (err !== null) {
        this.$toast({ message: err, position: 'bottom' })
        return
      }

      if (xyHelper.isNullOrEmpty(this.saler)) {
        err = '请选择售达方'
      }

      if (err !== null) {
        this.$toast({ message: err, position: 'bottom' })
        return
      }

      var reallist = []
      if (!xyHelper.isNullOrEmpty(this.list)) {
        for (var i = 0; i < this.list.length; i++) {
          if (!xyHelper.isNullOrEmpty(this.list[i].batcode) &&
            !xyHelper.isNullOrEmpty(this.list[i].startbundle) &&
            !xyHelper.isNullOrEmpty(this.list[i].endbundle)) {
            reallist.push(this.list[i])
          }
        }
      }

      if (xyHelper.isNullOrEmpty(reallist)) {
        err = '请输入出库产品资源'
      }

      if (err !== null) {
        this.$toast({ message: err, position: 'bottom' })
      } else {
        var postdata = {
          lpn: this.lisenseplate,
          sellerid: this.saler.id,
          list: JSON.stringify(reallist),
          deliveryType: 0
        }
        // 提交
        this.$http.post('api/v1/Product/', postdata).then(function (res) {
          // var data = null
          // if (xyHelper.isNullOrEmpty(res)) {

          // } else {
          //   data = xyHelper.tryParseJson(res)
          // }

          var data = res

          if (!xyHelper.isNullOrEmpty(data)) {
            if (data.status === 0) {
              vue.$toast({message: data.msg, position: 'middle'})
            } else {
              vue.$toast({message: '成功出库' + data + '件产品', iconClass: 'icon iconfont icon-success'})

              // 清除所有数据
              vue.lisenseplate = ''
              vue.saler = ''
              vue.batcodes = []
              vue.list = [
                {
                  'batcode': null,
                  'startbundle': null,
                  'endbundle': null,
                  'material': null,
                  'spec': null,
                  'quantity': null,
                  'weight': null,
                  'pieceweight': null
                }
              ]
            }
          } else {
            vue.$toast({message: '出库失败', position: 'middle'})
          }
        })
      }
    }
  },
  watch: {
    'list': {
      deep: true,
      handler: function (oldValue, newValue) {
        for (var index = 0; index < this.list.length; index++) {
          if (this.list[index].startbundle > 0 && this.list[index].endbundle - this.list[index].startbundle >= 0) {
            this.list[index].quantity = this.list[index].endbundle - this.list[index].startbundle + 1
            this.list[index].weight = this.list[index].pieceweight * this.list[index].quantity
          }
        }
      }
    }
  },
  components: {
  }
}
</script>
<style scoped>
body{background: #f5f5f5;}
</style>
<style>
.tips{
  height: 20px;
  color:red;
  text-align: left;
  background: white;
}
.stockout .mycellcontainer .mint-cell.mint-field .mint-cell-wrapper .mint-cell-title{ 
  width: 50px !important;
  text-align: right !important;
  padding: 0 10px 0 0 !important;
  white-space: nowrap !important;
}
.stockout .mint-cell.mint-field.mycell .mint-cell-wrapper .mint-cell-title{
  width: 50px !important;
  text-align: right !important;
  white-space: nowrap !important;
}
.stockout .mint-cell.mint-field.mycell .mint-cell-wrapper{
  padding:0 0 0 10px !important;
}
.list .listitem {
  padding:10px 0 0 0;
  min-height: 30px;
  text-align: center;
}
.list .listitem .listleft{
  width:70px;
  height: 50px;
  float:left;
  border: 1px solid #999;
  padding:5px 0;
}
.list .listitem .listleft .icon-add{
  font-size: 30px;
  line-height: 1;
}
.batcode{
  line-height: 30px;
}
.list .listitem .listright{
  line-height:45px;
}
.list .listitem .listright input{
  height: 30px;
  width: 50px;
  line-height:30px;
  margin:0 5px;
}
.list .listitem .count label{
  padding:0 10px;
}
.list .listitem .h-line{
  margin-top:10px;
}
.stockout .mint-popup {
  width: 100%;
  text-align: center;
  backface-visibility: hidden;
}
</style>
