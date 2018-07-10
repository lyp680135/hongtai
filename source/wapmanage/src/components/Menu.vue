<template>
    <div>
        <mt-popup v-model="popupVisible" position="right">
            <mt-cell v-for="(ml,index) in menuList" :key="index" :title="ml.name" :to="ml.url" @click.native="hidemenu()"></mt-cell>
            <mt-cell title="退出系统" to="/Exit"></mt-cell>
        </mt-popup>
        <button class="menubtn" @click="showmenu()">
            <img src="../assets/menuwhite.png">
            <p>菜单</p>
        </button>
    </div>
</template>


<script>
import SystemConfig from '@/plugins/systemConfig'

export default {
  name: 'menulist',
  data () {
    return {
      popupVisible: false,
      menuList: []
    }
  },
  mounted () {
    let vue = this
    this.$http.get('api/v1/Menu').then(res => {
      vue.menuList = res
    }).catch(e => {
      console.info(e)
    })
  },
  methods: {
    showmenu () {
      this.popupVisible = true
    },
    toHome () {
      window.location.href = SystemConfig.getConfig().Domain_WAP
    },
    hidemenu () {
      console.log('123')
      this.popupVisible = false
    }
  }
}
</script>

<style>
.menubtn {
  border: none;
  position: fixed;
  right: 12px;
  bottom: 120px;
  background: #ff4249;
  width: 47px;
  height: 47px;
  border-radius: 50%;
  z-index:9999;
}

.menubtn img {
  width: 13px;
}

.menubtn p {
  color: #fff;
  font-size: 10px;
  letter-spacing: 2px;
  margin-left: -3px;
  color: #fff;
  font-size: 10px;
  letter-spacing: 2px;
  margin-left: -3px;
  padding: 0;
  margin: 0;
}
</style>