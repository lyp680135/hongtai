<template>
    <div>
        <mt-cell v-for="(item,index2) in list" :key="index2" is-link  @click.native="onCellClick(item)">
                <div slot="title" class="iteminfo">
                    <div class="bianhao">
                        <p>{{item.Consignor}}
                            <br/>
                        {{item.PrintNo}}
                    </p>                              
                    </div>
                    <div class="itemcode">
                        {{item.Number}}件
                    </div>
                        <div class="itemcode">
                    {{item.Createtime}}
                    </div>
                </div>                    
        </mt-cell>               
        <!-- </mt-loadmore> -->
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
import xyHelper from '../plugins/xyHelper'
function loadOpenedPage (vue, batcode) {
  var datalist = []
  vue.$http
    .get(
      'api/v1/Detail/?batcode=' + batcode
    )
    .then(function (res) {
      if (res.status === 0) {
        vue.nodataflag = true
        return
      }
      if (xyHelper.isNullOrEmpty(res)) {
        vue.$toast({ message: '没有更多数据了', position: 'bottom' })
      } else {
        datalist = xyHelper.tryParseJson(res)
        vue.list = datalist
      }
    })
    .catch(function (error) {
      if (error) {
        vue.nodataflag1 = true
      }
    })
}
export default {
  name: 'Detail',
  data () {
    return {
      title: '打印详情',
      nodataflag: false,
      disallowBottomLoaded: false,
      list: [],
      totalSelectedNum: 0
    }
  },
  methods: {
    onCellClick (item) {
      this.$router.push('/WarrantyBook/' + item.PrintNo + '')
      // this.$router.push({path: '/WarrantyBook', query: {p: item.PrintNo, iswater: 1}})
    }
  },
  mounted: function () {
    loadOpenedPage(this, this.$route.params.batcode)
  }
}
</script>
<style scoped>
.opened .mint-cell {
  background-image: none;
}
.opened .mint-cell .mint-cell-wrapper {
  background-image: none !important;
  background-size: 0 0 !important;
  padding: 0px !important;
}
.opened .mint-button {
  height: inherit;
}
.mint-cell-wrapper {
  background-image: none !important;
  background-size: 0 0 !important;
  padding: 0px !important;
}
.mint-cell-title {
  padding-left: 10px;
}
.mint-cell-value {
  padding-right: 10px;
}
.tabbar {
  background-color: #fff;
  display: -webkit-box;
  display: -ms-flexbox;
  display: flex;
  text-align: center;
}
.tabbar .tabitem {
  font-size: 15px;
}
.tabitem {
  display: block;
  padding: 7px 0;
  -webkit-box-flex: 1;
  -ms-flex: 1;
  flex: 1;
  text-decoration: none;
}
.tabbar .selected {
  border-bottom: 3px solid #26a2ff;
  color: #26a2ff;
  margin-bottom: -3px;
}

.mint-tab-container {
  top: 3px;
  font-size: 12px;
}

.mint-tab-container p {
  padding: 2px 0px;
  margin: 0px;
  font-size: 12px;
}

.minus,
.plus {
  display: inline-block;
  width: 28px;
  height: 28px;
  line-height: 28px;
  font-size: 20px;
  border-radius: 5px;
  border: 1px solid #aaa;
  text-align: center;
  color: #aaa;
  cursor: pointer;
}
.number {
  width: 40px;
  border: 0;
  box-shadow: 0 0 1px #7a7a7b;
  height: 30px;
  border-radius: 3px;
  text-align: center;
  font-size: 13px;
}

.bottom_css {
  /*底部功能栏*/
  width: 100%;
  height: 50px;
  background-color: #fff;
  text-align: left;
  color: #999;
  position: fixed;
  bottom: 0px;
  z-index: 999;
  padding-top: 10px;
  background-image: linear-gradient(
    180deg,
    #d9d9d9,
    #d9d9d9 50%,
    transparent 50%
  );
  background-size: 120% 1px;
  background-repeat: no-repeat;
  background-position: top left;
}
.bottom_css .selected {
  float: left;
  padding-left: 15px;
}
.bottom_css .selected p {
  margin: 0px;
}
.bottom_css .selected label {
  font-size: 14px;
  color: red;
  margin-right: 2px;
}
.bottom_css .botton {
  float: right;
}
.bottom_css .botton button {
  margin-right: 15px;
}
.nodata {
  position: fixed;
  height: 100%;
  width: 100%;
  text-align: center;
  vertical-align: middle;
  color: #d9d9d9;
  font-size: 14px;
}
.nodata ul {
  padding: 0px;
  margin: 0px;
  list-style: none;
  display: table-cell;
  vertical-align: middle;
  height: 50vh;
  width: 10000vh;
}
.nodata ul li {
  color: #999;
  font-size: 14px;
}
.opened_itemtitle {
  margin: 0px;
  background: #f5f4f4;
  display: flex;
  justify-content: space-between;
  padding: 10px;
}
.iteminfo {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.itemcode {
  flex: 1px;
  text-align: center;
  line-height: 20px;
  font-size: 14px;
}
.mint-button.calcbtn {
  height: 32px;
  position: relative;
  top: 1px;
}
 .search input{
   width: 70%;
   margin:10px 10px;
   font-size:1.2em;
  height:33px;
  border-radius:5px;
  border:1px solid #c8cccf;
  color:#333; 
  outline:0; 
  text-align:center; 
  display:inline-block;
 }
 .btnsearch{
   width: 20%;
   display: inline-block;
   height: 36px;
 }
 .bianhao{
   width: 30%;
  
 }
 .bianhao p{
   font-size: 14px !important;
   line-height: 20px;
   /* padding: 5px 0; */
 }
</style>