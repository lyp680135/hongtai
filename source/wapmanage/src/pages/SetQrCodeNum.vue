<template>
  <div class="hello">
    <mt-cell v-for="(wk,index) in workShopList" :key="index" :to="'/SetQrCodeNumData/'+wk.Id" :title="wk.Name" is-link></mt-cell>
  </div>
</template>

<script>

export default {
  name: 'SetQrCodeNum',
  data () {
    return {
      title: '选择授权车间',
      workShopList: []
    }
  },
  methods: {
  },
  mounted () {
    let vue = this

    this.$http.get('api/v1/WorkShop').then(res => {
      vue.workShopList = res
    }).catch(e => {
      console.info(e)
    })

    this.$emit('viewLoaded', this)
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h1, h2 {
  font-weight: normal;
}
a{
    text-decoration: none;
    color: #0066cc;
}
</style>
