

var config = {
    // ��������
    alias: {
        'json': '/project/js/json',
        'jquery': '/project/js/jquery',
        'DDTree': '/project/js/DDTree/DDTree',
         'Selector': '/project/js/Selector/Selector'
        'myRepeater': '/project/js/myRepeater/myRepeater'

    },

    // ·������
    paths: {
        'gallery': 'https://a.alipayobjects.com/gallery'
    },

    // ��������
    vars: {
        'locale': 'zh-cn'
    },



    // Ԥ������
    preload: [
        this.JSON ? '' : 'json'
        , this.$ ? '' : 'jquery'
  ],

    // ����ģʽ
    debug: true,

    // Sea.js �Ļ���·��
    base: '/project/js/',

    // �ļ�����
    charset: 'utf-8'
}

seajs.config(config);
seajs.use('jquery');
