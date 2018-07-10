<template>
  <div id="app">
    <HeaderCompontent :title="title" :rightbutton="rightbutton" v-on:rightClick="onRightClick" :is-index="isindex"></HeaderCompontent>
    <transition :name="transitionName">
      <keep-alive exclude="SetQrCodeNumData,TemplatPreView,SelectBundleCode,SetSaleSeller,WarrantyBook">
        <router-view v-on:viewLoaded="onViewLoaded" />
      </keep-alive>
    </transition>
  </div>
</template>
<script type="text/babel">
import HeaderCompontent from './components/Header.vue'
import Router from 'vue-router'
import SystemConfig from '@/plugins/systemConfig'

Router.prototype.go = function () {
  this.isBack = true
  window.history.go(-1)
}

// 或者你可以新建一个方法
Router.prototype.goBack = function () {
  this.isBack = true
  this.go(-1)
}

export default {
  name: 'app',
  data () {
    return {
      transitionName: 'slide-right',
      title: SystemConfig.getConfig().Name,
      rightbutton: null,
      contentview: null,
      isindex: true
    }
  },
  mounted () {},
  methods: {
    onViewLoaded (view) {
      this.contentview = view
      this.title = view.title
      this.rightbutton = view.rightbutton
    },
    onRightClick () {
      if (this.contentview !== undefined) {
        this.contentview.onRightClick()
      }
    }
  },
  components: {
    HeaderCompontent
  },
  watch: {
    '$route' (to, from) {
      // 如果isBack为true时，证明是用户点击了回退，执行slide-right动画
      let isBack = this.$router.isBack
      if (isBack) {
        this.transitionName = 'slide-right'
        this.title = to.meta.title
        if (from.name === 'StockOutput') {
          this.rightbutton = null
        }
      } else {
        this.transitionName = 'slide-left'
      }
      // 做完回退动画后，要设置成前进动画，否则下次打开页面动画将还是回退
      this.$router.isBack = false
      if (to.name === 'HelloWorld') {
        this.isindex = true
      } else {
        this.isindex = false
      }
    }
  }
}
</script>

<style>
#app {
  font-family: "Avenir", Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
}

.mint-field .mint-cell-title{ 
  width: 50px;
  text-align: right;
  padding: 0 10px 0 0;
  white-space: nowrap;
}

/**切换动画**/
.slide-right-enter-active{
  animation: bounce-right-in 0.5s;
  -webkit-animation: bounce-right-in 0.5s;
}
.slide-right-leave-active {
  animation: bounce-right-out 0.2s;
  -webkit-animation: bounce-right-out 0.2s;
}
.slide-left-enter-active{
  animation: bounce-left-in 0.5s;
  -webkit-animation: bounce-left-in 0.5s;
}
.slide-left-leave-active {
  animation: bounce-left-out 0.2s;
  -webkit-animation: bounce-left-out 0.2s;
}

@keyframes bounce-right-in {
    0% {
        transform: translate3d(100%, 0, 0);
    }
    100% {
        transform: translate3d(0, 0, 0);
    }
}
@-webkit-keyframes bounce-right-in {
    0% {
        -webkit-transform: translate3d(100%, 0, 0);
    }
    100% {
        -webkit-transform: translate3d(0, 0, 0);
    }
}
@keyframes bounce-right-out {
    100% {
        opacity: 1;
    }
    0% {
        opacity: 0.8;
    }
}
@-webkit-keyframes bounce-right-out {
    100% {
        opacity: 1;
    }
    0% {
        opacity: 0.8;
    }
}

@keyframes bounce-left-in {
    100% {
        transform: translate3d(0, 0, 0);
    }
    0% {
        transform: translate3d(100%, 0, 0);
    }
}
@-webkit-keyframes bounce-left-in {
    100% {
        -webkit-transform: translate3d(0, 0, 0);
    }
    0% {
        -webkit-transform: translate3d(100%, 0, 0);
    }
}
@keyframes bounce-left-out {
    100% {
        opacity: 1;
    }
    0% {
        opacity: 0.8;
    }
}
@-webkit-keyframes bounce-left-out {
    100% {
        opacity: 1;
    }
    0% {
        opacity: 0.8;
    }
}
/**切换动画结束**/
</style>