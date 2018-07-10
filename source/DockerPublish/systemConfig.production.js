const systemConfig = {
  getConfig: () => {
    var con = {'Domain': 'http://lgzxsteel.com/',
      'Domain_PC': 'http://www.lgzxsteel.com/',
      'Domain_WAP': 'http://m.lgzxsteel.com/',
      'Domain_WAPManage': 'http://manage.lgzxsteel.com/',
      'Domain_WebApi': 'http://api.lgzxsteel.com/',
      'Domain_SJBFile': 'http://114.215.102.245:8104/',
      'Domain_QRCode': 'http://cnzui.com/z?p={0}-{1}',
      'Name': '涟钢振兴质保书系统',
      'NameEn': 'Quality management system'}
    return con
  }
}

export default systemConfig
