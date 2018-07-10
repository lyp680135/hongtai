const systemConfig = {
  getConfig: () => {
    var con = {'Domain': 'http://dev.xiaoyutt.com:7051/',
      'Domain_PC': 'http://dev.xiaoyutt.com:7051/',
      'Domain_WAP': 'http://dev.xiaoyutt.com:7052/',
      'Domain_WAPManage': 'http://dev.xiaoyutt.com:7054/',
      'Domain_WebApi': 'http://dev.xiaoyutt.com:7053/',
      'Domain_SJBFile': 'http://114.215.102.245:8104/',
      'Domain_QRCode': 'http://cnzui.com/z?p={0}&c={1}',
      'Name': '鸿泰钢铁质保书系统',
      'NameEn': 'Quality management system'}
    return con
  }
}

export default systemConfig
