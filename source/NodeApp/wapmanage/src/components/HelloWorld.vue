<template>
  <div class="hello">
    <h1>菜单列表</h1>
    <mt-cell v-for="(ml,index) in menuList" :key="index" :title="ml.name" :to="ml.url"></mt-cell>
    <mt-cell title="退出系统" to="/Exit"></mt-cell>
  </div>
</template>

<script>
import MenuCompontent from '@/components/Menu'
import SystemConfig from '@/plugins/systemConfig'
export default {
  name: 'HelloWorld',
  data () {
    return {
      title: SystemConfig.getConfig().Name,
      menuList: []
    }
  },
  mounted () {
    this.$emit('viewLoaded', this)
    let vue = this
    this.$http.get('api/v1/Menu').then(res => {
      vue.menuList = res
    }).catch(e => {
      console.info(e)
    })
  },
  activated: function () {
    this.$emit('viewLoaded', this)
  },
  components: {
    MenuCompontent
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h1{
      font-size: 15px;
    font-weight: 600;
    text-align: center;
}
</style>
