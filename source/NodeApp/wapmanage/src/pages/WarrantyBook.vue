<template>
<div>
      <div id="warranty" v-show="creatingflag">
        <div id="loadimg" style="color:#eaeaea;padding:50px 0px;font-size: 20px;text-align:center">
              <vueProgress value="15" :options="{maxValue:15,valRate:100/15,text:function(value){
          return parseInt(15-value);
        },pathColors: ['#e0e0e0','#26a2ff'],circleLineCap: 'round',varyStrokeArray: [5,8],duration:15000}"></vueProgress>
              <br />{{msg}}
        </div>
      </div>
      
      <div class="buttom_css" v-show="certready">
      <mt-button type="primary" @click="saveAsMobile()">下载质保书</mt-button>
      <div class="tips">温馨提示：<br />质保书下载成功后，默认保存在手机相册中<br />部分手机由于浏览器限制，可能不能下载质保书，
        <br />请尝试用系统自带浏览器，长按以上图片保存到相册</div>
      </div>

    <div class="nodata" v-show="nodataflag">
      <ul>
        <li>
          <img src="../assets/nodata.png" alt="" width="138" height="79"/>
        </li>
        <li>
            没有相关记录
        </li>
      </ul>
    </div>
 </div>
</template>

<script>
// import xyHelper from '../plugins/xyHelper'
import vueProgress from '../components/vueProgressBar.vue'
import SystemConfig from '@/plugins/systemConfig'

export default {
  name: 'WarrantyBook',
  data () {
    return {
      title: '质保书生成与预览',
      creatingflag: false,
      certready: false,
      nodataflag: false,
      iswater: 0,
      PrintNo: '',
      data: {
        radius: 20,
        percent: 51,
        PermitNo: '',
        Address: '',
        AddressEn: '',
        CheckPerson: '',
        EntryPerson: '',
        Client: '',
        ClientEn: '',
        DeliveryCompany: '',
        DeliveryState: '',
        DomainPc: '',
        Gbname: '',
        Logo: '',
        MaterilaName: '',
        OutDate: '',
        SignetAngle: '',
        Standard: '',
        List: []
      },
      logo: '',            // LOGO
      zxp: '',
      qrimg: '',           // 二维码地址
      qualityIcon: '',     // 质量图标
      chapter: '',         // 章
      fileDomain: '',
      rqcodeUrl: '',
      issave: false,
      msg: '质保书正在生成中...'
    }
  },
  methods: {
    saveAsMobile: function () {
      // 点击下载
      genHtmlImg(this, 'download')
    }
  },
  mounted: function () {
    var vue = this

    this.$emit('viewLoaded', this)
    // 从上个页面传过来的打印编号
    var printno = this.$route.params.p
    // var shuiyin = this.$route.params.iswater
    // if (parseInt(shuiyin) === 1) {
    //   vue.msg = '质保书预览中...'
    // }
    vue.PrintNo = printno
    vue.creatingflag = true
    genHtmlImg(vue, 'preview')
  },
  components: {
    vueProgress
  }
}

// 跟据HTML生成图片
function genHtmlImg (vue, op) {
  var htmlurl = 'api/v1/Quality/'
  var params = {
    'printno': vue.PrintNo
  }

  if (op === 'download') {
    params.iswater = 1
    vue.$indicator.open('下载中')
  }

  vue.$http.get(htmlurl + '?p=' + params.printno + '&iswater=' + params.iswater, {'responseType': 'blob'}).then(function (blob) {
    var binaryData = []
    binaryData.push(blob)
    // var objectURL = window.URL.createObjectURL(new Blob(binaryData, {type: 'image/jpeg'}))

    var objectURL = window.URL.createObjectURL(blob)
    var image = new Image()
    image.src = objectURL

    if (op === 'download') {
      if (myBrowser() === 'WeChat' || myBrowser() === 'Safari') {
        var htmlurl = 'api/v1/Quality/'
        var params = {
          'printno': vue.PrintNo
        }
        vue.$http.post(htmlurl + '?p=' + params.printno + '&iswater=1' + '&flag=' + 1).then(function (obj) {
          window.location.href = SystemConfig.getConfig().Domain_WAP + 'Cert/?p=' + obj.Id + '-' + obj.Checkcode
        }).catch(function (e) {
          console.log('Oops, error' + e)
        })
      } else {
        oDownLoad(image.src)
      }
    } else {
      document.getElementById('loadimg').style.display = 'none'
      image.style.width = '100%'
      document.getElementById('warranty').appendChild(image)
    }
    vue.creatingflag = true
    vue.certready = true
    vue.$indicator.close()
  }).catch(function (e) {
    console.log('Oops, error' + e)
    vue.creatingflag = false
    vue.nodataflag = true
    vue.$indicator.close()
  })
}

// 判断浏览器类型
function myBrowser () {
  var userAgent = navigator.userAgent // 取得浏览器的userAgent字符串
  var isOpera = userAgent.indexOf('Opera') > -1
  if (isOpera) {
    return 'Opera'
  } // 判断是否Opera浏览器
  if (userAgent.indexOf('Firefox') > -1) {
    return 'FF'
  } // 判断是否Firefox浏览器
  if (userAgent.indexOf('Chrome') > -1) {
    return 'Chrome'
  }
  if (userAgent.indexOf('Safari') > -1) {
    return 'Safari'
  } // 判断是否Safari浏览器
  if (userAgent.indexOf('compatible') > -1 && userAgent.indexOf('MSIE') > -1 && !isOpera) {
    return 'IE'
  } // 判断是否IE浏览器
  if (userAgent.indexOf('Trident') > -1) {
    return 'Edge'
  } // 判断是否Edge浏览器
  if (userAgent.indexOf('MicroMessenger') > -1) {
    return 'WeChat'
  } // 判断是否为微信客户端
}

// IE浏览器图片保存本地
function SaveAs5 (imgURL) {
  var oPop = window.open(imgURL, '', 'width=1, height=1, top=5000, left=5000')
  for (; oPop.document.readyState !== 'complete';) {
    if (oPop.document.readyState === 'complete') break
  }
  oPop.document.execCommand('SaveAs')
  oPop.close()
}

function oDownLoad (url) {
  if (myBrowser() === 'IE' || myBrowser() === 'Edge') {
    SaveAs5(url)
  } else {
    download(url)
  }
}
// 谷歌，360极速等浏览器下载
function download (src) {
  var $a = document.createElement('a')
  $a.setAttribute('href', src)
  $a.setAttribute('download', '')

  var evObj = document.createEvent('MouseEvents')
  evObj.initMouseEvent('click', true, true, window, 0, 0, 0, 0, 0, false, false, true, false, 0, null)
  $a.dispatchEvent(evObj)
}
</script>
<style scoped>
#warranty{
  text-align: center;
}
.buttom_css{
  padding:15px;
}
.buttom_css button{
  width:100%;
}
.tips{
  padding:10px 5px;
  color:#999;
}
</style>
