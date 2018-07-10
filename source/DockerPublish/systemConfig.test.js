const systemConfig = {
  getConfig: () => {
    var con = {'Domain': 'http://dev.xiaoyutt.com:7031/',
      'Domain_PC': 'http://dev.xiaoyutt.com:7031/',
      'Domain_WAP': 'http://dev.xiaoyutt.com:7032/',
      'Domain_WAPManage': 'http://dev.xiaoyutt.com:7034/',
      'Domain_WebApi': 'http://dev.xiaoyutt.com:7033/',
      'Domain_SJBFile': 'http://114.215.102.245:8104/',
      'Domain_QRCode': 'http://cnzui.com/z?p={0}&c={1}',
      'Name': '涟钢振兴质保书系统',
      'NameEn': 'Quality management system'}
    return con
  }
}

export default systemConfig
