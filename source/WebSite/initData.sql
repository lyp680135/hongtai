/*
Navicat MySQL Data Transfer

Source Server         : local
Source Server Version : 50719
Source Host           : localhost:3306
Source Database       : warranty

Target Server Type    : MYSQL
Target Server Version : 50719
File Encoding         : 65001

Date: 2017-11-22 11:13:48
*/

SET FOREIGN_KEY_CHECKS=0;
 
-- ----------------------------
-- Records of mngadmin
-- ----------------------------
INSERT INTO `mngadmin` VALUES ('1', null, '1', 'GLY', '[1]', '', '\0', null, null, null, null, null, '4297f44b13955235245b2497399d7a93', null, null, '管理员','','Admin');
 
-- ----------------------------
-- Records of mngdepartmentclass
-- ----------------------------
INSERT INTO `mngdepartmentclass` VALUES ('1', '\0', '0', '技术部', '1', '\0', '0', '1', '1');
 
-- ----------------------------
-- Records of mngmenuclass
-- ----------------------------
INSERT INTO `mngmenuclass` VALUES ('1', '0', '菜单设置', '2', '\0', '\0', '2', '3,2,1,', '0', '3', '/Manage/Authority/Menu');
INSERT INTO `mngmenuclass` VALUES ('2', '3', '系统设置', '1', '\0', '\0', '3', '3,2,', '0', '2', '');
INSERT INTO `mngmenuclass` VALUES ('3', '3', '系统管理', '0', '\0', '\0', '0', '3,', '0', '1', '');
INSERT INTO `mngmenuclass` VALUES ('4', '2', '人员管理', '1', '\0', '\0', '3', '3,4,', '0', '6', '');
INSERT INTO `mngmenuclass` VALUES ('5', '0', '增加人员', '2', '\0', '\0', '4', '3,4,5,', '0', '7', '/Manage/Authority/UserAdd');
INSERT INTO `mngmenuclass` VALUES ('6', '3', '人员列表', '2', '\0', '\0', '4', '3,4,6,', '0', '8', '/Manage/Authority/User');
INSERT INTO `mngmenuclass` VALUES ('7', '0', '修改', '3', '\0', '', '6', '3,4,6,7,', '0', '10', null);
INSERT INTO `mngmenuclass` VALUES ('8', '0', '删除', '3', '\0', '', '6', '3,4,6,8,', '0', '11', null);
INSERT INTO `mngmenuclass` VALUES ('9', '0', '查看', '3', '\0', '', '6', '3,4,6,9,', '0', '9', null);
INSERT INTO `mngmenuclass` VALUES ('10', '1', '个人信息修改', '1', '\0', '\0', '3', '3,10', '0', '12', null);
INSERT INTO `mngmenuclass` VALUES ('11', '0', '个人资料修改', '2', '\0', '\0', '10', '3,10,11,', '0', '13', '/Manage/Authority/Edit');
INSERT INTO `mngmenuclass` VALUES ('12', '0', '部门设置', '2', '\0', '\0', '2', '3,2,12,', '0', '4', '/Manage/Authority/Department');
INSERT INTO `mngmenuclass` VALUES ('13', '0', '权限组设置', '2', '\0', '\0', '2', '3,2,13,', '0', '5', '/Manage/Authority/Permissiongroup');
 
-- ----------------------------
-- Records of mngpermissiongroup
-- ----------------------------
INSERT INTO `mngpermissiongroup` VALUES ('1', '\0', '管理员', '管理员', '\0', '1');
INSERT INTO `mngpermissiongroup` VALUES ('2', '\0', '业务经理', '业务经理', '\0', '2');
INSERT INTO `mngpermissiongroup` VALUES ('3', '\0', '品质主管', '品质主管', '\0', '3');
INSERT INTO `mngpermissiongroup` VALUES ('4', '\0', '质量员', '质量员', '\0', '4');
INSERT INTO `mngpermissiongroup` VALUES ('5', '\0', '入库操作员', '入库操作员', '\0', '5');
INSERT INTO `mngpermissiongroup` VALUES ('6', '\0', '出库操作员', '出库操作员', '\0', '6');
-- ----------------------------
-- Records of mngpermissiongroupset
-- ----------------------------
INSERT INTO `mngpermissiongroupset` VALUES ('1', '1', '13');
INSERT INTO `mngpermissiongroupset` VALUES ('2', '1', '12');
INSERT INTO `mngpermissiongroupset` VALUES ('3', '1', '11');
INSERT INTO `mngpermissiongroupset` VALUES ('4', '1', '10');
INSERT INTO `mngpermissiongroupset` VALUES ('5', '1', '9');
INSERT INTO `mngpermissiongroupset` VALUES ('6', '1', '7');
INSERT INTO `mngpermissiongroupset` VALUES ('7', '1', '6');
INSERT INTO `mngpermissiongroupset` VALUES ('8', '1', '4');
INSERT INTO `mngpermissiongroupset` VALUES ('9', '1', '3');
INSERT INTO `mngpermissiongroupset` VALUES ('10', '1', '2');
INSERT INTO `mngpermissiongroupset` VALUES ('11', '1', '1');
INSERT INTO `mngpermissiongroupset` VALUES ('12', '1', '8');
INSERT INTO `mngpermissiongroupset` VALUES ('13', '1', '5');


SET FOREIGN_KEY_CHECKS=1;