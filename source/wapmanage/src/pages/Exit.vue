<template>
</template>

<script>
import SystemConfig from '@/plugins/systemConfig'
export default {
  name: 'Exit',
  data () {
    return {
    }
  },
  mounted () {
    localStorage.Authorization = ''
    window.location.href = SystemConfig.getConfig().Domain_WAP + 'LogOff'
  }
}
</script>