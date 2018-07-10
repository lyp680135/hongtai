import axios from 'axios'
import Base64Parent from 'js-base64'
import SystemConfig from '@/plugins/systemConfig'
import Vue from 'vue'

const Base64 = Base64Parent.Base64

const Axios = axios.create({
  baseURL: SystemConfig.getConfig().Domain_WebApi,
  timeout: 100000,
  headers: {
    'Content-Type': 'application/x-www-form-urlencoded;'
  }
})

var loading = false
// POST传参序列化(添加请求拦截器)
Axios.interceptors.request.use(config => {
  if (config.data !== undefined && config.url.indexOf('ashx') < 0) {
    config.data = 'value=' + Base64.encode(JSON.stringify(config.data))
  }

  if (config.showloading) {
    Vue.$indicator.open()
    loading = true
  }

  // 拼接Jwt认证
  if (localStorage.Authorization && config.method !== 'options') {
    if (config.url.indexOf('http://') < 0) {
      config.headers.common['Authorization'] = 'Bearer ' + localStorage.Authorization
    }
  }
  console.info(config.baseURL)
  return config
}, error => {
  alert('错误的传参', 'fail')
  return Promise.reject(error)
})

// 返回状态判断(添加响应拦截器)
Axios.interceptors.response.use(res => {
  if (loading) {
    Vue.$indicator.close()
    loading = false
  }

  try {
    return JSON.parse(res.data.data)
  } catch (e) {
    return res.data
  }
}, error => {
  if (loading) {
    Vue.$indicator.close()
  }

  if (error.response !== undefined) {
    if (error.response.status === 401) {
      // 401 说明 token 验证失败
      // 可以直接跳转到登录页面，重新登录
      // window.location.href = SystemConfig.getConfig().Domain_WAP + 'Login'
    } else {
       // do something
    }
    throw error.response.data.Msg
    // 返回 response 里的错误信息
    // return error.response.data
  } else {
    let reCon = {
      Status: 0,
      Msg: '请求失败：' + error,
      Data: error
    }
    throw reCon.Msg
  }
})

export default {
  install (Vue) {
    Object.defineProperty(Vue.prototype, '$http', { value: Axios })
  },
  Axios () {
    return Axios
  }
}
