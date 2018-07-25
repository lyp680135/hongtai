const systemConfig = {
  getConfig: () => {
    var con = {'Domain': 'http://hongtai.31goods.com/',
      'Domain_PC': 'http://hongtai.31goods.com/',
      'Domain_WAP': 'http://hongtaim.31goods.com/',
      'Domain_WAPManage': 'http://hongtaimanage.31goods.com/',
      'Domain_WebApi': 'http://hongtaiapi.31goods.com/',
      'Domain_SJBFile': 'http://114.215.102.245:8104/',
      'Domain_QRCode': 'http://cnzui.com/x?p={0}-{1}',
      'Name': '鸿泰钢铁质保书系统',
      'NameEn': 'Quality management system'}
    return con
  }
}

export default systemConfig
