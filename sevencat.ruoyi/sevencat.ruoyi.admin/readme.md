### admin 主要是整个框架的装配工作

整体使用autofac做di,会进行自动扫描所有的类似java的注解。

一部分注册移到了AppModule ，管道也放在里面 工厂类放在config目录下面。 demo目录是测试用的

#### minio

minio封装里面可以endpoint和url不一致（内部问题），也解决了通过路径访问的设置， 具体参考OssConfig的写法

### sys 主要是系统模块

### common 通用模块，基础组件

### 不同模块之间互相调用的话，抽象出一个api 比如sevencat.ruoyi.sys.api,把数据库定义和需要暴露给外部的接口在这里体现。

### 整个项目目前需要依赖redis ，以后如果有需要，可以依赖其他的分步式cache