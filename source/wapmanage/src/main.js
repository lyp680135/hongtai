// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
/* eslint-disable no-new */
import Vue from 'vue'
import App from './App'
import router from './router'
import AxiosPlugin from './plugins/axiosPlugin'
import SystemConfig from '@/plugins/systemConfig'
import MintUI from 'xy-mint-ui'
import 'xy-mint-ui/lib/style.css'
import '../static/css/common.css'
Vue.use(MintUI)
Vue.use(AxiosPlugin)
Vue.config.productionTip = false

new Vue({
  created () {
    document.title = SystemConfig.getConfig().Name
  },
  el: '#app',
  router,
  template: '<App/>',
  components: { App }
})
