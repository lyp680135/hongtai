<template>
  <div class="hello">
    <h1>{{ msg }}</h1>
    <a href="javascript:window.history.go(-1)"><< 返回上页</a>
  </div>
</template>

<script>
export default {
  name: 'Error',
  data () {
    return {
      msg: '系统出现错误！'
    }
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
