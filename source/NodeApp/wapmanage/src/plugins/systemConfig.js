const systemConfig = {
  getConfig: () => {
    var con = {'Domain': 'http://localhost:49503/',
      'Domain_PC': 'http://localhost:49503/',
      'Domain_WAP': 'http://localhost:8003/',
      'Domain_WAPManage': 'http://localhost:8080/',
      'Domain_WebApi': 'http://localhost:41178/',
      'Domain_SJBFile': 'http://114.215.102.245:8104/',
      'Domain_QRCode': 'http://cnzui.com/x?p={0}-{1}',
      'Name': '鸿泰钢铁质保书系统',
      'NameEn': 'Quality management system'}
    return con
  }
}

export default systemConfig
