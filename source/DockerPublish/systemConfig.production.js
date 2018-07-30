const systemConfig = {
  getConfig: () => {
    var con = {'Domain': 'http://jshtsteel.com/',
      'Domain_PC': 'http://www.jshtsteel.com/',
      'Domain_WAP': 'http://m.jshtsteel.com/',
      'Domain_WAPManage': 'http://manage.jshtsteel.com/',
      'Domain_WebApi': 'http://api.jshtsteel.com/',
      'Domain_SJBFile': 'http://114.215.102.245:8104/',
      'Domain_QRCode': 'http://cnzui.com/x?p={0}-{1}',
      'Name': '鸿泰钢铁质保书系统',
      'NameEn': 'Quality management system'}
    return con
  }
}

export default systemConfig
