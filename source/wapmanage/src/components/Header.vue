<template>
    <div>
        <mt-header :title="title">
            <router-link to="" @click.native="goback" slot="left" v-if="(!this.isIndex)">
                <mt-button icon="back">返回</mt-button>
            </router-link>
            <mt-button class="rightbtn" slot="right" v-if="(this.rightbutton != null)" @click="onRightClick">
              <i :class="'icon iconfont icon-'+this.rightbutton.image" slot="icon"></i>
            </mt-button>
        </mt-header>
    </div>
</template>

<script>
/* import axios from 'axios' */

export default {
  name: 'menulist',
  data () {
    return {
    }
  },
  props: {
    title: String,
    rightbutton: {
      title: String,
      image: String
    },
    isIndex: false
  },
  mounted () {
    console.log(this.rightbutton.image)
  },
  methods: {
    goback () {
      this.$router.go(-1)
    },
    onRightClick () {
      this.$emit('rightClick', null)
    }
  }
}
</script>
<style>
.mint-header button label{
    font-size:12px;
}
.rightbtn .iconfont{
  width:20px;
  font-size:22px;
}
</style>
