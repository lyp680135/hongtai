import Vue from 'vue'
import Router from 'vue-router'
import AxiosPlugin from '@/plugins/axiosPlugin'
import SystemConfig from '@/plugins/systemConfig'
import HelloWorld from '@/components/HelloWorld'
import StockOutput from '@/pages/StockOutput'
import SelectTopSaler from '@/pages/SelectTopSaler'
import Error from '@/pages/Error'
import SetAuth from '@/pages/SetAuth'
import Exit from '@/pages/Exit'
import SallerAuth from '@/pages/SallerAuth'
import SetQrCodeNum from '@/pages/SetQrCodeNum'
import SetQrCodeNumData from '@/pages/SetQrCodeNumData'
import TemplatPreView from '@/pages/TemplatPreView'
import WarrantyBook from '@/pages/WarrantyBook'
import SetSaleSeller from '@/pages/SetSaleSeller'
import NoRuleStockOutput from '@/pages/NoRuleStockOutput'
import SelectBundleCode from '@/pages/SelectBundleCode'
import WarrantyAuth from '@/pages/WarrantyAuth'
import WarrantyPrint from '@/pages/WarrantyPrint'
import Detail from '@/pages/Detail'
Vue.use(Router)

var router = new Router({
  mode: 'history',
  routes: [
    {
      path: '/',
      name: 'HelloWorld',
      component: HelloWorld
    },
    {
      path: '/Error',
      name: 'Error',
      component: Error
    },
    {
      path: '/SetAuth',
      name: 'SetAuth',
      component: SetAuth
    },
    {
      path: '/Exit',
      name: 'Exit',
      component: Exit
    },
    {
      path: '/StockOutput',
      name: 'StockOutput',
      meta: {
        title: '产品出库',
        keepAlive: false
      },
      component: StockOutput
    },
    {
      path: '/NoRuleStockOutput',
      name: 'NoRuleStockOutput',
      meta: {
        title: '非尺出库',
        keepAlive: false
      },
      component: NoRuleStockOutput
    },
    {
      path: '/SelectTopSaler',
      name: 'SelectTopSaler',
      component: SelectTopSaler
    },
    {
      path: '/SallerAuth',
      name: 'SallerAuth',
      component: SallerAuth
    },
    {
      path: '/SetQrCodeNum',
      name: 'SetQrCodeNum',
      meta: {
        title: '选择授权车间',
        keepAlive: false
      },
      component: SetQrCodeNum
    },
    {
      path: '/SetQrCodeNumData/:id',
      name: 'SetQrCodeNumData',
      meta: {
        keepAlive: false
      },
      component: SetQrCodeNumData
    },
    {
      path: '/SelectBundleCode/:type/:batcode/:index/:deliverytype',
      name: 'SelectBundleCode',
      meta: {
        keepAlive: false
      },
      component: SelectBundleCode
    },
    {
      path: '/TemplatPreView',
      name: 'TemplatPreView',
      component: TemplatPreView
    },
    {
      path: '/WarrantyBook/:p',
      name: 'WarrantyBook',
      meta: {
        title: '质保书生成与预览',
        keepAlive: false
      },
      component: WarrantyBook
    },
    {
      path: '/SetSaleSeller',
      name: 'SetSaleSeller',
      meta: {
        title: '售达方管理',
        keepAlive: false
      },
      component: SetSaleSeller
    },
    {
      path: '/WarrantyAuth',
      name: 'WarrantyAuth',
      meta: {
        title: '授权资源',
        keepAlive: false
      },
      component: WarrantyAuth
    },
    {
      path: '/WarrantyPrint',
      name: 'WarrantyPrint',
      meta: {
        title: '授权资源',
        keepAlive: false
      },
      component: WarrantyPrint
    },
    {
      path: '/Detail/:batcode',
      name: 'Detail',
      meta: {
        title: '打印详情',
        keepAlive: false
      },
      component: Detail
    }
  ]
})

router.beforeEach(({ name }, from, next) => {
  if (name !== 'SetAuth' && name !== 'Error' && name !== 'Exit' && name !== 'WarrantyBook') {
    // 判断JWT_TOKEN
    if (localStorage.Authorization) {
      try {
        AxiosPlugin.Axios().get('api/TokenCheck').then(res => {
          next()
        }).catch(e => {
          if (e !== undefined && e.toString() === 'JWT验证失败') {
            window.location.href = SystemConfig.getConfig().Domain_WAP + 'Login'
          }
        })
      } catch (e) {
        console.info('router拦截器出现异常： ' + e)
      }
    } else {
      window.location.href = SystemConfig.getConfig().Domain_WAP + 'Login'
    }
  } else {
    next()
  }
})

export default router
