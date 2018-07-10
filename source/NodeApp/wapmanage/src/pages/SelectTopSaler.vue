<template>
  <div class="selectseller page-indexlist-wrapper">
    <mt-index-list v-show="!nodataflag">
      <mt-index-section v-for="(section,index) in list" v-bind:key="section.name" :index="section.name">
        <mt-cell v-for="(item,pos) in section.list" v-bind:key="item.id"
         :title="item.name" :label="item.mobile"
         @click.native="onCellClick(item)" is-link>
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

var postData = {
  'PageIndex': 1,
  'PageSize': 20
}

function loadPage (vue) {
  vue.$http.get('api/v1/Seller?PageIndex=' + postData.PageIndex + '&PageSize=' + postData.PageSize).then(function (res) {
    var datalist = []
    if (xyHelper.isNullOrEmpty(res)) {
      if (postData.PageIndex === 1) {
        vue.$toast({ message: '没有符合条件的数据', position: 'bottom' })
      } else {
        vue.$toast({ message: '没有更多数据了', position: 'bottom' })
      }
    } else {
      datalist = xyHelper.tryParseJson(res)
      if (datalist.length === 0) {
        vue.disallowBottomLoaded = true
      } else if (datalist.length < postData.PageSize) {
        vue.disallowBottomLoaded = false
      } else {
        vue.disallowBottomLoaded = false
      }
    }

    if (postData.PageIndex <= 1) {
      vue.list = []
    }

    vue.list.push.apply(vue.list, datalist)

    if (vue.list.length === 0) {
      vue.nodataflag = true
    }

    postData.PageIndex++
  }).catch(function (error) {
    if (error) {
      vue.$toast({ message: '数据请求失败', position: 'bottom' })
      if (postData.PageIndex === 1) {
        vue.nodataflag = true
      }
    }
  })
}

export default {
  name: 'StockOutput',
  data () {
    return {
      title: '选择售达方',
      nodataflag: false,
      list: [
        {
          name: '',
          list: []
        }
      ]
    }
  },
  mounted () {
    this.$emit('viewLoaded', this)
    postData.PageIndex = 1
    loadPage(this)
  },
  methods: {
    onCellClick (saler) {
      Bus.$emit('topsalerSelected', saler)
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
</style>
