<template>
  <div class="setseller page-indexlist-wrapper">
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
    <div id="mask" v-show="editflag || addflag"></div>
    <div class="editbox" v-show="editflag || addflag">
        <div class="mint-msgbox-header">
            <div class="mint-msgbox-title" v-if="editflag">编辑售达方</div>
            <div class="mint-msgbox-title" v-if="addflag">添加售达方</div>
            <img class="closebtn" src="../assets/close.png" @click="closeClick" />
        </div>
        <div class="mint-msgbox-content">
            <mt-field label="名称" placeholder="请输入售达方名称" v-model="selected.name"></mt-field>
            <div class="remark-title">联系方式</div>
            <mt-field disable-clear type="textarea" rows="2" placeholder="请添加联系方式，多个手机号请以逗号、空格或换行隔开" class="last-cell remark" v-model="selected.mobiles"></mt-field>
            <div class="btn-container">
              <div class="btnitem">
                <mt-button size="large" type="primary" @click.native="submit">确定</mt-button>
              </div>
              <div class="btnitem">
                <mt-button size="large" type="danger" @click.native="deleteClick">删除</mt-button>
              </div>
            </div>
        </div>
    </div>
  </div>
</template>

<script>
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
    vue.nodataflag = false
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
  name: 'SetSaleSeller',
  data () {
    return {
      title: '售达方管理',
      rightbutton: {
        title: '',
        image: 'add'
      },
      nodataflag: false,
      editflag: false,
      addflag: false,
      list: [
        {
          name: '',
          list: []
        }
      ],
      selected: {
        id: 0,
        name: '',
        mobiles: ''
      }
    }
  },
  mounted () {
    this.$emit('viewLoaded', this)
    postData.PageIndex = 1
    loadPage(this)
  },
  methods: {
    onCellClick (saler) {
      this.selected.id = saler.id
      this.selected.name = saler.name
      this.selected.mobiles = saler.mobile
      this.editflag = true
    },
    hidemask () {
      this.editflag = false
      this.addflag = false
    },
    closeClick () {
      this.editflag = false
      this.addflag = false
    },
    submit () {
      var vue = this
      var err = null
      if (xyHelper.isNullOrEmpty(this.selected.name)) {
        err = '请输入售达方名称'
      }

      if (err !== null) {
        this.$toast({ message: err, position: 'bottom' })
        return
      }

      if (xyHelper.isNullOrEmpty(this.selected.mobiles)) {
        err = '请输入售达方联系方式'
      }

      if (err !== null) {
        this.$toast({ message: err, position: 'bottom' })
      }

      if (err !== null) {
        this.$toast({ message: err, position: 'bottom' })
      } else {
        var postdata = {
          id: this.selected.id,
          name: this.selected.name,
          mobiles: this.selected.mobiles
        }

        var url = ''
        if (this.selected.id > 0) {
          url = 'api/v1/Seller/' + this.selected.id
          this.$http.put(url, postdata, {
            showloading: true
          }).then(function (res) {
            vue.onSubmitResult(res)
          })
        } else {
          url = 'api/v1/Seller/'
          this.$http.post(url, postdata, {
            showloading: true
          }).then(function (res) {
            vue.onSubmitResult(res)
          })
        }
      }
    },
    deleteClick () {
      var vue = this
      vue.$messagebox.confirm('确定要删除这个售达方吗？').then(function () {
        var url = 'api/v1/Seller/' + vue.selected.id
        vue.$http.delete(url).then(function (res) {
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
              vue.$toast({message: '删除成功', iconClass: 'icon iconfont icon-success'})
              vue.editflag = false
              postData.PageIndex = 1
              loadPage(vue)
            }
          } else {
            vue.$toast({message: '删除失败了', position: 'middle'})
          }

          vue.editflag = false
          vue.addflag = false
        })
      })
    },
    onSubmitResult (res) {
      var data = null
      if (xyHelper.isNullOrEmpty(res)) {

      } else {
        data = xyHelper.tryParseJson(res)
      }

      if (!xyHelper.isNullOrEmpty(data)) {
        if (data.status === 0) {
          this.$toast({message: data.msg, position: 'middle'})
        } else {
          if (this.selected.id) {
            this.$toast({message: data.data, iconClass: 'icon iconfont icon-success'})
          } else {
            this.$toast({message: '添加成功', iconClass: 'icon iconfont icon-success'})
          }
          this.editflag = false
          this.addflag = false
        }
      } else {
        if (this.selected.id) {
          this.$toast({message: '更新失败', position: 'middle'})
        } else {
          this.$toast({message: '添加失败', position: 'middle'})
        }
      }

      postData.PageIndex = 1
      loadPage(this)
    },
    onRightClick () {
      this.addflag = true
      this.selected.id = 0
      this.selected.name = ''
      this.selected.mobiles = ''
    }
  }
}
</script>

<style>
.setseller.page-indexlist-wrapper,.setseller .mint-indexlist,.setseller .mint-indexlist-content{
  min-height:500px;
}
.setseller .mint-indexlist-nav {
    border-left: none !important;
    background-color: transparent !important;
}
.setseller .mint-indexsection-index {
    background-color: #f5f5f5 !important;
    padding: 0 0 0 15px !important;
    line-height: 25px !important;
    color: #999 !important;
}
.setseller .mint-indexlist-content {
    margin-right: 0px !important;
}
.setseller .mint-cell{
  min-height:60px !important;
}
.setseller .mint-cell.mint-field{
  min-height:40px !important;
}
.setseller .nodata{
  position: fixed;
  height: 100%;
  width: 100%;
  text-align: center;
  vertical-align: middle;
  color: #d9d9d9;
  font-size:14px;
} 
.setseller .nodata ul{
  padding:0px;margin:0px;list-style:none;
  display: table-cell;
  vertical-align: middle;
  height: 80vh;
  width: 10000vh;
}
.setseller .nodata ul li{
  color:#999;
  font-size:14px;
}
.setseller #mask {
    position: fixed;
    left: 0;
    top: 0;
    z-index: 100;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.4);
}
.setseller .remark-title{
    padding-left:15px;
    background: white;
    display: flex;
    font-size: 15px;
    line-height: 50px;
    overflow: hidden;
    height:40px;
}
.setseller .remark{
    padding-bottom: 10px;
}
.setseller .remark .mint-cell-wrapper{
    background-image: none;
}
.setseller .closebtn{
    position: absolute;
    right:12px;
    top:12px;
    padding:5px;
    width: 15px;
    height: 15px;
}
.setseller .editbox{
    position: fixed;
    top: 50%;
    left: 50%;
    z-index: 999;
    -webkit-transform: translate3d(-50%,-50%,0);
    transform: translate3d(-50%,-50%,0);
    background-color: #fff;
    width: 85%;
    border-radius: 3px;
    font-size: 16px;
    -webkit-user-select: none;
    overflow: hidden;
    -webkit-backface-visibility: hidden;
    backface-visibility: hidden;
    -webkit-transition: .2s;
    transition: .2s;
}
.setseller .mint-msgbox-content{
    padding:5px 0 0 0 !important;
}
.setseller .mint-field textarea{
    width:100%;
    border:none;
    padding:8px 5px;
    height:98px;
    outline:none;
    font-size:15px;
    background:#f5f5f5;
    border-radius: 3px;
    resize: none;
}
.setseller .mint-field .mint-cell-title{
    width:65px;
}
.btnitem{ width:50%; text-align: center; float:left; margin-bottom: 10px;}
.btnitem button { width:90%;}
</style>
