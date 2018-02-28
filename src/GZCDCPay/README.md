数据分中心支付平台
======

## 安装调试
1. 安装 dotnet [https://dotnet.github.io/](https://dotnet.github.io/)
2. 下载源码 git clone https://github.com/watson1029/gzcdc-pay.git
3. 恢复依赖包 dotnet restore 
4. 运行 dotnet run
- - - 
## 部署
### IIS
### Docker
- - -
## API
### 下单接口
**地址** http://pay.gz-eport.com/ 或 http://pay.gz-eport.com/Payment/

**方法** Get Post

**传入参数**

* *OrderId* 订单ID，32长度字符，会直接传到微信、支付宝
* *Amount* 订单金额，整数，单位为分
* *Description* 商品描述，会在页面上显示
* *ApplicantName* 付款人显示名称，会在页面上显示
* *AppId* 应用ID，对应应用的key用于签名
* *CallbackUrl* 支付结果页面回调的url，参数采用querystring的形式
* *NotifyUrl* 支付结果通知Url，参数采用json形式
* *Account* 账号选择（haicheng，~~gzcdc~~) 暂时只支持haicheng账号
* *Note* 附加信息，只会存在数据库中，用于核对订单
* *AppId* 应用ID
* *NonceStr* 随机字符串32位
* *Signature* 签名（见下文签名算法）

**回调、通知时带的参数**
* *Result* 查询请求状态码 (SUCCESS, FAIL)
* *Message* 查询请求状态码，显示出错原因
    * *OK* 没有错误
    * *ArgumentsInvalid* 参数错误
    * *SignatureInvalid* 签名错误
    * *OrderNotFound* 没有此订单号
    * *OrderDuplicated* 订单号重复
    * *AppIdNotFound* 没有找到对一个AppId
    * *SystemError* 系统错误
* *AppId* 应用ID
* *OrderId* 订单ID
* *Status* 订单状态
    * *Preparing* 已在平台下单，未支付
    * *Paying*  用户已扫码，未支付
    * *Success* 交易成功
    * *NotPaid* 用户未支付
    * *Closed*  交易关闭
    * *Refunded* 已经退款
    * *Revoked* 已撤销（刷卡支付才会有的状态）
    * *Errored* 支付失败（如银行扣款失败）
* *NonceStr* 随机字符串32位
* *Signature* 签名（见下文签名算法）
* *LastUpdated* 最后更新时间yyyyMMddHHmmss


### 查询接口


**地址** http://pay.gz-eport.com/api/Payment/OrderStatus

**方法** Get Post

**传入参数**
* *OrderId* 订单ID，32长度字符，会直接传到微信、支付宝
* *AppId* 应用ID，对应应用的key用于签名
* NonceStr
* Signature

**返回数据** json格式，同回调参数一致
* *Result* 查询请求状态码 (SUCCESS, FAIL)
* *Message* 查询请求状态码，显示出错原因
    * *OK* 没有错误
    * *ArgumentsInvalid* 参数错误
    * *SignatureInvalid* 签名错误
    * *OrderNotFound* 没有此订单号
    * ~~*OrderDuplicated* 订单号重复~~
    * *AppIdNotFound* 没有找到对一个AppId
    * *SystemError* 系统错误
* *AppId* 应用ID
* *OrderId* 订单ID
* *Status* 订单状态
    * *Preparing* 已在平台下单，未支付
    * *Paying*  用户已扫码，未支付
    * *Success* 交易成功
    * *NotPaid* 用户未支付
    * *Closed*  交易关闭
    * *Refunded* 已经退款
    * *Revoked* 已撤销（刷卡支付才会有的状态）
    * *Errored* 支付失败（如银行扣款失败）
* *NonceStr* 随机字符串32位
* *Signature* 签名（见下文签名算法）
* *LastUpdated* 最后更新时间yyyyMMddHHmmss





### 签名测试接口

**地址** http://pay.gz-eport.com/Test/

**方法** Get, Post

**传入参数**
* *Content* 任意内容
* *AppId* 应用ID，对应应用的key用于签名
* *NonceStr* 随机字符串32位
* *Signature* 签名（见下文签名算法）

**输出**
* 若正确 ```Passed. Signature {signature} is correct.```
* 若错误 ```Failed. Signature should be {signature}.```


### 签名验证接口1

**地址** http://pay.gz-eport.com/Test/TestSignature

**方法** Get

**传入参数**
* *Content* 任意内容
* *AppId* 应用ID，对应应用的key用于签名, 测试期间可以使用 testAppId 作为 AppId， testApiKey 作为对应的 Key
* *NonceStr* 随机字符串32位
* *Signature* 签名（见下文签名算法）

**输出**
* 若验证错误，返回 
```javascript
{
    "Result": "FAIL", 
    "Message": "..."  
}
```
* 若验证正确, 执行业务逻辑返回，测试者应再验证Signature正确性
```javascript
{
    "Result": "SUCCESS", 
    "Message": "OK",  
    "AppId" : "...",
    "NonceStr": "...",
    "Signature": "...",
    "Content": "..."
}
```

### 签名验证接口2

**地址** http://pay.gz-eport.com/Test/TestSignatureWithArbitraryArguments

**方法** Get

**必须传入的参数**
* *AppId* 应用ID，对应应用的key用于签名, 测试期间可以使用 testAppId 作为 AppId， testApiKey 作为对应的 Key
* *Signature* 签名（见下文签名算法）

**输出**
* 若正确 ```Passed. Signature {signature} is correct.```
* 若错误 ```Failed. Signature should be {signature}.```


### 签名算法
参考微信公众平台 https://pay.weixin.qq.com/wiki/doc/api/native.php?chapter=4_3

1. 将所有非空参数以url格式按字典序列排序(不需要对字段进行urlencode),signature不参与运算
2. 然后并上&key=ApiKey
3. 再计算MD5的值

**Example:** [SignatureService.CalculateSignature](./Servcies/SignatureServices.cs#L21-L37)
