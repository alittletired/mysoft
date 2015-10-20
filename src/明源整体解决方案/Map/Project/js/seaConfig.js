

var config = {
    // 别名配置
    alias: {
        'json': '/project/js/json',
        'jquery': '/project/js/jquery',
        'DDTree': '/project/js/DDTree/DDTree',
         'Selector': '/project/js/Selector/Selector'
        'myRepeater': '/project/js/myRepeater/myRepeater'

    },

    // 路径配置
    paths: {
        'gallery': 'https://a.alipayobjects.com/gallery'
    },

    // 变量配置
    vars: {
        'locale': 'zh-cn'
    },



    // 预加载项
    preload: [
        this.JSON ? '' : 'json'
        , this.$ ? '' : 'jquery'
  ],

    // 调试模式
    debug: true,

    // Sea.js 的基础路径
    base: '/project/js/',

    // 文件编码
    charset: 'utf-8'
}

seajs.config(config);
seajs.use('jquery');
