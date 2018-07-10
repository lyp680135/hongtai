<template>
</template>

<script>
/* eslint-disable no-new */

import xyHelper from '@/plugins/xyHelper'

export default {
  name: 'SetAuth',
  data () {
    return {}
  },
  mounted () {
    let authorizationQuery = xyHelper.queryString('s')
    if (authorizationQuery !== '') {
      localStorage.Authorization = authorizationQuery
    }

    // let systemConfig = xyHelper.queryString('con')
    // if (systemConfig !== '') {
    //   localStorage.systemConfig = decodeURIComponent(systemConfig)
    // }
    this.$router.push({path: '/', replace: true})
  }
}
</script>
