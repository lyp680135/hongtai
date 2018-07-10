<template>
  <div>
        <div class="tabbar">
            <div class="tabitem" :class="{selected : tabindex == 1}" @click="changeTab(1)">
                    可开
            </div>
            <div class="tabitem" :class="{selected : tabindex == 2}"  @click="changeTab(2)">
                    已开
            </div>
        </div> 
        <!-- tab-container -->
        <mt-tab-container v-model="tabindex">
            <mt-tab-container-item :id="1">
                <mt-loadmore :top-method="loadTop" :bottom-method="loadBottom" :bottom-all-loaded="disallowBottomLoaded" ref="loadmore">
                   
                    <div class="opened" v-for="(item,index) in list" :key="index">
                        <h3 class="opened_itemtitle"><label>{{item.Lpn}}</label> <label>{{item.Date}}</label></h3>
                        <mt-cell v-for="(child,index1) in item.list" :key="index1">
                          <div slot="title" class="iteminfo">
                            <div>
                              <p>{{child.Name}}<br/>{{child.MaterialName}}/{{child.Specname}}</p>
                              <p>授{{child.AuthNumber}}/剩{{child.Number-child.AuthNumber}}件</p>
                            </div>
                            <div class="itemcode">
                              {{child.Batcode}}
                            </div>
                          </div>
                          <div>
                               <mt-button size="small" class="calcbtn" type="default"  @click.native="minus(index,index1)">-</mt-button>
                               <input class="number"  type="number" v-model="child.SelectedNumber" @blur="calc(index,index1)"/>
                               <mt-button size="small" class="calcbtn"  type="default" @click.native="plus(index,index1)">+</mt-button>
                              件
                          </div>
                        </mt-cell>
                    </div>

                </mt-loadmore>
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
                <div class="bottom_css">
                    <div class="selected" v-show="totalSelectedNum>0">
                        <p>已选</p>
                        <label>{{totalSelectedNum}}</label>件
                    </div>
                    <div class="botton">
                    <mt-button type="primary" @click="auth()">授权</mt-button>
                    <mt-button type="primary" @click="genWarranty()">生成质保书</mt-button>
                    </div>
                </div>
            </mt-tab-container-item>
            <mt-tab-container-item :id="2">
                <!-- <mt-loadmore :top-method="loadTop1" :bottom-method="loadBottom1" :bottom-all-loaded="disallowBottomLoaded1" ref="loadmore1"> -->
                <div class="opened" v-for="(item,index1) in list1" :key="index1">
                    <h3 class="opened_itemtitle">{{item.sellername}}</h3>
                    <mt-cell v-for="(child,index2) in item.list" :key="index2">
                        <div slot="title" class="iteminfo">
                            <div>
                              <p>{{child.Name}}<br/>{{child.MaterialName}}/{{child.Specname}}</p>
                              <p>{{child.Number}}件</p>
                            </div>
                             <div class="itemcode">
                              {{child.Batcode}}
                            </div>
                        </div>
                        <div>
                        <mt-button class="default" @click="cancalAuth(child.Id)">撤消授权</mt-button>
                        </div>
                    </mt-cell>
                </div>
                <!-- </mt-loadmore> -->
                <div class="nodata" v-show="nodataflag1">
                    <ul>
                        <li>
                            <img src="../assets/nodata.png" alt="" width="138" height="79"/>
                        </li>
                        <li style="text-indent: 15px;">
                            没有相关记录
                        </li>
                    </ul>
                </div>
            </mt-tab-container-item>
        </mt-tab-container>
  </div>
</template>

<script>

import xyHelper from '../plugins/xyHelper'
// 可开
var openPostData = {
  'PageIndex': 1,
  'PageSize': 20
}
// 已开
var openedPostData = {
  'PageIndex': 1,
  'PageSize': 200
}
function loadOpenedPage (vue) {
  var datalist = []
  vue.$http.get('api/v1/SellerOpenedAuth/?PageSize=' + openedPostData.PageSize + '&PageIndex=' + openedPostData.PageIndex).then(function (res) {
    if (res.status === 0) {
      if (openedPostData.PageIndex === 1) {
        datalist = []
        vue.list1 = datalist
        vue.nodataflag1 = true
      }
      return
    }

    if (xyHelper.isNullOrEmpty(res)) {
      if (openedPostData.PageIndex === 1) {
        datalist = []
        vue.$toast({ message: '没有符合条件的数据', position: 'bottom' })
      } else {
        vue.$toast({ message: '没有更多数据了', position: 'bottom' })
      }
    } else {
      datalist = xyHelper.tryParseJson(res)
      if (datalist.length === 0) {
        vue.disallowBottomLoaded1 = true
      } else if (datalist.length < openedPostData.PageSize) {
        vue.disallowBottomLoaded1 = false
      } else {
        vue.disallowBottomLoaded1 = false
      }
    }
    if (openedPostData.PageIndex <= 1) {
      vue.list1 = []
    }
    vue.list1 = datalist
    if (vue.list1.length === 0) {
      vue.nodataflag1 = true
    }
    openedPostData.PageIndex++
  }).catch(function (error) {
    if (error) {
      if (openedPostData.PageIndex === 1) {
        vue.list1 = []
        vue.nodataflag1 = true
      }
    }
  })
}
function loadOpenPage (vue) {
  vue.$http.get('api/v1/SellerAuth/?PageSize=' + openPostData.PageSize + '&PageIndex=' + openPostData.PageIndex).then(function (res) {
    if (res.status === 0) {
      vue.$toast({ message: '没有更多数据了', position: 'bottom' })
      if (openPostData.PageIndex === 1) {
        vue.nodataflag = true
      }
      return
    }
    var datalist = []
    if (xyHelper.isNullOrEmpty(res)) {
      if (openPostData.PageIndex === 1) {
        vue.$toast({ message: '没有符合条件的数据', position: 'bottom' })
      } else {
        vue.$toast({ message: '没有更多数据了', position: 'bottom' })
      }
    } else {
      datalist = xyHelper.tryParseJson(res)
      if (datalist.length === 0) {
        vue.disallowBottomLoaded = true
      } else if (datalist.length < openPostData.PageSize) {
        vue.disallowBottomLoaded = false
      } else {
        vue.disallowBottomLoaded = false
      }
    }

    if (openPostData.PageIndex <= 1) {
      vue.list = []
    }

    vue.list = datalist
    if (vue.list.length === 0) {
      vue.nodataflag = true
    }
    openPostData.PageIndex++
  }).catch(function (error) {
    if (error) {
      vue.$toast({ message: '没有更多数据了', position: 'bottom' })
      if (openPostData.PageIndex === 1) {
        vue.nodataflag = true
      }
    }
  })
}
export default {
  name: 'SallerAuth',
  data () {
    return {
      title: '经销商授权',
      tabindex: 1,
      nodataflag: false,
      disallowBottomLoaded: false,
      list: [],
      totalSelectedNum: 0,
      list1: [],
      disallowBottomLoaded1: false,
      nodataflag1: false
    }
  },

  methods: {
    changeTab: function (index) {
      this.tabindex = index
      if (this.tabindex === 1) {
        openPostData.PageIndex = 1
        loadOpenPage(this)
      }
      if (this.tabindex === 2) {
        openedPostData.PageIndex = 1
        loadOpenedPage(this)
      }
    },
    minus: function (index, index1) {
      if (this.list[index].list[index1].SelectedNumber > 0) {
        this.list[index].list[index1].SelectedNumber = parseInt(this.list[index].list[index1].SelectedNumber) - 1
        totalNum(this)
      }
    },
    calc: function (index, index1) {
      try {
        this.list[index].list[index1].SelectedNumber = parseInt(this.list[index].list[index1].SelectedNumber)
        var num = this.list[index].list[index1].Number - this.list[index].list[index1].AuthNumber

        if (this.list[index].list[index1].SelectedNumber > num) {
          this.$toast({message: '授权件数不能大于剩余件数', position: 'bottom'})
          this.list[index].list[index1].SelectedNumber = num
        }
      } catch (e) {
        this.list[index].list[index1].SelectedNumber = 0
      }
      totalNum(this)
    },
    plus: function (index, index1) {
      var num = this.list[index].list[index1].Number - this.list[index].list[index1].AuthNumber
      if (this.list[index].list[index1].SelectedNumber < num) {
        this.list[index].list[index1].SelectedNumber = parseInt(this.list[index].list[index1].SelectedNumber) + 1
        totalNum(this)
      }
    },
    auth: function () {
      var vue = this
      if (this.totalSelectedNum === 0) {
        this.$toast({message: '请选择授权物资', position: 'bottom'})
        return
      }
      var array = []
      for (var i = 0; i < this.list.length; i++) {
        for (var j = 0; j < this.list[i].list.length; j++) {
          if (this.list[i].list[j].SelectedNumber > 0) {
            array.push(this.list[i].list[j])
          }
        }
      }
      this.$messagebox.prompt('被授权人手机号').then(({ value, action }) => {
        if (action === 'confirm') {
          if (!value) {
            this.$toast({message: '请输入手机号', position: 'middle'})
          } else {
            var myreg = /^[1][3,4,5,7,8,9][0-9]{9}$/
            if (value.length > 11 || !myreg.test(value)) {
              this.$toast({message: '请输入正确的手机号', position: 'middle'})
              return
            }
            var requeatData = JSON.stringify(array)
            vue.$http.post('api/v1/SellerAuth/?requestData=' + requeatData + '&mobile=' + value).then(function (res) {
              if (res.status === 1) {
                vue.$toast({message: '授权成功', iconClass: 'icon iconfont icon-success'})
                openPostData.PageIndex = 1
                loadOpenPage(vue)
                vue.totalSelectedNum = 0
              } else {
                vue.$toast({message: res.data, position: 'middle'})
              }
            }).catch(function (error) {
              if (error) {
                vue.$toast({ message: '数据请求失败：' + error, position: 'bottom' })
              }
            })
          }
        }
      })
    },
    cancalAuth: function (authid) {
      var vue = this
      vue.$messagebox.confirm('确认撤消此经销商及下级所有经销商对此条物资的权限吗？').then(function (action) {
        if (action === 'confirm') {
          var postdata = {authids: authid}
          vue.$http.put('api/v1/SellerAuth/?requestData=' + JSON.stringify(postdata)).then(function (res) {
            if (res.status === 1) {
              vue.$toast({message: '撤消授权成功', iconClass: 'icon iconfont icon-success'})
              openedPostData.PageIndex = 1
              loadOpenedPage(vue)
            } else {
              vue.$toast({message: res.data, position: 'middle'})
            }
          }).catch(function (error) {
            if (error) {
              vue.$toast({ message: '数据请求失败：' + error, position: 'bottom' })
            }
          })
        }
      })
    },
    genWarranty: function () {
      // var vue = this
      if (this.totalSelectedNum === 0) {
        this.$toast({message: '请选择物资', position: 'bottom'})
        return
      }
      var array = []
      var msg = ''
      for (var i = 0; i < this.list.length; i++) {
        for (var j = 0; j < this.list[i].list.length; j++) {
          var item = this.list[i].list[j]
          if (item.SelectedNumber > 0) {
            if (array.length > 0) {
              if (!array.contains('Name', item.Name) ||
                !array.contains('MaterialName', item.MaterialName)) {
                msg = '同品名、同牌号、同类型的物资才能生成质保书，请重新选择'
                break
              } else {
                // 如果不属于同一个规格的，则另起一行新记录
                var find = false
                for (var k = 0; k < array.length; k++) {
                  if (array[k].Name === item.Name &&
                  array[k].MaterialName === item.MaterialName &&
                  array[k].Specid === item.Specid &&
                  array[k].Batcode === item.Batcode) {
                    array[k].SelectedNumber += item.SelectedNumber
                    find = true
                    break
                  }
                }
                if (!find) {
                  array.push(item)
                }
              }
            } else {
              array.push(item)
            }
          }
        }
      }

      if (msg.length > 0) {
        this.$toast({message: '同品名、同牌号、同类型的物资才能生成质保书，请重新选择', position: 'bottom'})
        return
      }
      localStorage.GenWarrantylist = JSON.stringify(array)
      // 生成质保书
      this.$router.push('/TemplatPreView')
    },
    loadBottom: function () {
      this.$refs.loadmore.onBottomLoaded()
      loadOpenPage(this)
    },
    loadTop: function () {
      openPostData.PageIndex = 1
      this.$refs.loadmore.onTopLoaded()
      loadOpenPage(this)
    },
    loadBottom1: function () {
      this.$refs.loadmore1.onBottomLoaded()
      loadOpenedPage(this)
    },
    loadTop1: function () {
      openedPostData.PageIndex = 1
      this.$refs.loadmore1.onTopLoaded()
      loadOpenedPage(this)
    }
  },
  mounted: function () {
    loadOpenPage(this)
    // loadOpenedPage(this)
  }
}
function totalNum (vue) {
  var num = 0
  for (var i = 0; i < vue.list.length; i++) {
    for (var j = 0; j < vue.list[i].list.length; j++) {
      if (vue.list[i].list[j].SelectedNumber > 0) {
        num += parseInt(vue.list[i].list[j].SelectedNumber)
      }
    }
  }
  vue.totalSelectedNum = parseInt(num)
}
</script>
<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.opened .mint-cell{
  background-image:none;
}
.opened .mint-cell .mint-cell-wrapper{
  background-image: none !important;
  background-size:0 0  !important;
  padding:0px !important;
}
.opened .mint-button{
    height:inherit;
}
.mint-cell-wrapper{
  background-image: none !important;
  background-size:0 0  !important;
  padding:0px !important;
}
.mint-cell-title{
  padding-left:10px;
}
.mint-cell-value{
  padding-right:10px;
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

.mint-tab-container{
    top:3px;
    font-size:12px;
}

.mint-tab-container p{
    padding:2px 0px;
    margin:0px;
    font-size:12px;
}

.minus,.plus{
    display:inline-block;
    width:28px;height:28px;line-height:28px;
    font-size:20px;
    border-radius: 5px;
    border:1px solid #aaa;
    text-align:center;
    color:#aaa;
    cursor:pointer;
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

.bottom_css {
    /*底部功能栏*/
    width:100%;
    height: 50px;
    background-color: #fff;
    text-align: left;
    color: #999;
    position: fixed;
    bottom:0px;
    z-index:999;
    padding-top:10px;
    background-image: linear-gradient(180deg, #d9d9d9, #d9d9d9 50%, transparent 50%);
    background-size: 120% 1px;
    background-repeat: no-repeat;
    background-position: top left;
}
.bottom_css .selected{
    float:left;
    padding-left:15px;
   
}
.bottom_css .selected p{
    margin:0px;
}
.bottom_css .selected label{
    font-size:14px;color:red;
    margin-right:2px;
}
.bottom_css .botton{
    float:right;
}
.bottom_css .botton button{
    margin-right:15px;
}
.nodata{
  position: fixed;
  height: 100%;
  width: 100%;
  text-align: center;
  vertical-align: middle;
  color: #d9d9d9;
  font-size:14px;
} 
.nodata ul{
  padding:0px;margin:0px;list-style:none;
  display: table-cell;
  vertical-align: middle;
  height: 50vh;
  width: 10000vh;
}
.nodata ul li{
  color:#999;
  font-size:14px;
}

.opened_itemtitle{
    margin:0px;
    background:#f5f4f4;
    display:flex;justify-content:space-between;padding:10px
}
.iteminfo{
    display: flex;
    justify-content: space-between;
    align-items: center;
}
.itemcode{
  flex:1px;text-align:center;
}
.mint-button.calcbtn{height:32px;position:relative;top:1px}

</style>
