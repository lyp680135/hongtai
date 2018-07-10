<template>
  <div class="selectseller page-indexlist-wrapper">
    <mt-index-list v-show="!nodataflag">
      <mt-index-section>
        <mt-cell v-for="(item,index) in list" v-bind:key="item.bundlecode"
         :title="item.bundlecode" 
         @click.native="onCellClick(item)" is-link>
            <span v-bind:class="{fontcolor:item.ifoutput==='已出货'}">{{item.ifoutput}}</span> 
        </mt-cell>
      </mt-index-section>
    </mt-index-list>
    <div class="nodata" v-show="nodataflag">
        <ul>
          <li>
            <img src="../assets/nodata.png" alt="" width="138" height="79"/>
          </li>
          <li style="text-indent: 15px;">
              没有相关记录
          </li>
        </ul>
    </div>
  </div>
</template>

<script>
import Bus from '../event/bus'
import xyHelper from '../plugins/xyHelper'
function loadpage (vue) {
  vue.$http.get('api/v1/ProductOutput?batcode=' + vue.batcode + '&deliverytype=' + vue.deliverytype + '').then(res => {
    if (xyHelper.isNullOrEmpty(res)) {
      vue.$toast({ message: '没有更多数据了', position: 'bottom' })
    } else {
      var jObject = xyHelper.tryParseJson(res)
      vue.list = jObject
      if (res.status === 0) {
        vue.nodataflag = true
      }
    }
  }).catch(err => {
    if (err) {
      vue.$toast({ message: '数据请求失败', position: 'bottom' })
    }
  })
}
export default {
  name: 'SelectBundleCode',
  data () {
    return {
      title: '选择捆号',
      nodataflag: false,
      list: [],
      batcode: this.$route.params.batcode,
      type: this.$route.params.type,
      index: this.$route.params.index,
      deliverytype: this.$route.params.deliverytype
    }
  },
  mounted () {
    loadpage(this)
    this.$emit('viewLoaded', this)
  },
  methods: {
    onCellClick (saler) {
      saler.batcode = this.$route.params.batcode
      saler.type = this.$route.params.type
      saler.index = this.$route.params.index
      saler.deliverytype = parseInt(this.$route.params.deliverytype)
      Bus.$emit('bundlecodeselected', saler)
      this.$router.go(-1)
    }
  }
}
</script>

<style>
.selectseller.page-indexlist-wrapper,.selectseller .mint-indexlist,.selectseller .mint-indexlist-content{
  min-height:500px;
}
.selectseller .mint-indexlist-nav {
    border-left: none !important;
    background-color: transparent !important;
}
.selectseller .mint-indexsection-index {
    background-color: #f5f5f5 !important;
    padding: 0 0 0 15px !important;
    line-height: 25px !important;
    color: #999 !important;
}
.selectseller .mint-indexlist-content {
    margin-right: 0px !important;
}
.selectseller .mint-cell{
  min-height:60px !important;
}
.selectseller .nodata{
  position: fixed;
  height: 100%;
  width: 100%;
  text-align: center;
  vertical-align: middle;
  color: #d9d9d9;
  font-size:14px;
} 
.selectseller .nodata ul{
  padding:0px;margin:0px;list-style:none;
  display: table-cell;
  vertical-align: middle;
  height: 80vh;
  width: 10000vh;
}
.selectseller .nodata ul li{
  color:#999;
  font-size:14px;
}
.fontcolor{
  color: red
}
/* .mint-cell-title{
    flex: 0.9;
}
.mint-cell-wrapper{
    padding:0 60px;
} */
</style>
