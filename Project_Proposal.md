# MoneyTracker 个人记账本 — 项目方案说明

---

## 一、项目背景

### 1.1 项目概述

MoneyTracker 是一款面向学生群体的桌面个人记账软件，采用 C# WinForm 技术栈开发。旨在帮助学生用户快速记录日常消费、查看月度支出分布、设定预算并生成统计报表，从而培养良好的消费习惯和理财意识。

### 1.2 项目动机

当前市面上的记账软件多为手机 App 或 Web 应用，存在以下问题：

- **功能冗余**：大量社交、理财推荐等无关功能干扰核心记账体验
- **隐私顾虑**：数据上传云端，用户对隐私安全存在担忧
- **平台限制**：部分学校网络环境受限，Web 应用访问不稳定

本项目定位为**轻量级本地桌面记账工具**，数据完全存储在本地 SQLite 数据库中，无需联网、无隐私泄露风险，启动即用、用完即走。

### 1.3 项目目标

- 提供 **3秒内完成一笔账单记录** 的快速记账体验
- 提供 **月度消费分布可视化**，帮助学生了解资金去向
- 提供 **预算设定与超支预警**，辅助理性消费决策
- 支持 **数据导出为 Excel**，便于汇总和分享

---

## 二、功能需求分析

### 2.1 用户角色

本系统仅设单一角色：**学生用户**。不区分管理员和普通用户。

### 2.2 用户故事

| 编号 | 用户故事 | 优先级 |
|------|---------|--------|
| US-01 | 作为学生，我想快速添加一笔消费记录（选分类→填金额→保存），以便随时记账不遗漏 | MVP |
| US-02 | 作为学生，我想按月查看我的账单列表，以便回顾当月消费明细 | MVP |
| US-03 | 作为学生，我想查看月度各分类支出的饼图，以便了解钱花在哪里 | MVP |
| US-04 | 作为学生，我想按分类筛选账单，以便查看某一类别的具体消费 | V2 |
| US-05 | 作为学生，我想编辑或删除我已录入的账单，以便修正错误 | V2 |
| US-06 | 作为学生，我想将账单数据导出为 Excel，以便做进一步分析或汇报 | V2 |
| US-07 | 作为学生，我想记录收入（如兼职工资），以便全面掌握财务状况 | V3 |
| US-08 | 作为学生，我想查看月度收支对比柱状图，以便评估节余情况 | V3 |

### 2.3 功能清单

#### MVP（最小可行产品）

| 功能模块 | 功能描述 | 对应US |
|---------|---------|--------|
| 添加账单 | 选择分类→输入金额→选择日期→填写备注→保存记录 | US-01 |
| 账单列表 | 按日期倒序展示当月所有账单，支持滚动浏览 | US-02 |
| 按月筛选 | 切换月份，列表和图表同步更新 | US-02 |
| 月度饼图 | 展示各分类支出金额和占比 | US-03 |
| 预设分类 | 应用首次启动自动插入学生常用分类（食堂/外卖/奶茶/交通等） | — |

#### V2（增强功能）

| 功能模块 | 功能描述 | 对应US |
|---------|---------|--------|
| 按分类筛选 | 在账单列表页按分类下拉筛选 | US-04 |
| 编辑账单 | 点击列表中某条记录进行金额/分类/备注修改 | US-05 |
| 删除账单 | 支持单条删除操作，带二次确认 | US-05 |
| Excel导出 | 将当前月份的账单数据导出为 .xlsx 文件 | US-06 |

#### V3（扩展功能）

| 功能模块 | 功能描述 | 对应US |
|---------|---------|--------|
| 收入记录 | 添加账单时可通过复选框标记为收入 | US-07 |
| 收支对比图 | 月度柱状图展示收入 vs 支出对比 | US-08 |

### 2.4 预设分类数据

| 分类名称 | 类型 | 排序 |
|---------|------|------|
| 食堂 | 支出 | 1 |
| 外卖 | 支出 | 2 |
| 奶茶零食 | 支出 | 3 |
| 日用品 | 支出 | 4 |
| 服饰 | 支出 | 5 |
| 交通 | 支出 | 6 |
| 学习用品 | 支出 | 7 |
| 娱乐 | 支出 | 8 |
| 通讯话费 | 支出 | 9 |
| 其他支出 | 支出 | 10 |
| 兼职工资 | 收入 | 11 |
| 其他收入 | 收入 | 12 |

---

## 三、技术选型理由

### 3.1 整体技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| C# | .NET 8.0 | 编程语言 |
| WinForm | .NET 8.0 | 桌面 UI 框架 |
| SQLite | — | 嵌入式数据库 |
| Dapper | 最新稳定版 | 轻量 ORM |
| EPPlus | 最新稳定版 | Excel 导出 |
| MSTest | — | 单元测试框架 |

### 3.2 选型理由分析

#### 为什么选择 WinForm？

- **课程对口**：本课程核心教学内容即为 WinForm 窗体与控件开发，选型与课程高度契合
- **上手快捷**：设计器可视化拖拽，适合快速搭建企业级管理界面
- **生态成熟**：原生 Chart 控件、DataGridView 等满足报表需求

#### 为什么选择 SQLite？

- **零配置**：无需安装数据库服务器，单文件存储，适合学生用户
- **便携性**：数据库文件（.db）可放在 U 盘或云盘同步
- **性能充足**：学生记账数据量小（月均<500条），SQLite 完全胜任
- **TDD 友好**：支持 `:memory:` 内存模式，测试建库零成本

#### 为什么选择 Dapper？

- **微型 ORM**：仅 ~500KB，不引入 EF Core 的庞大框架
- **手写 SQL**：保持对 SQL 的完全控制，查询优化灵活
- **自动映射**：查询结果自动按列名映射为 C# 对象，减少 70% 样板代码
- **原生异步**：支持 `QueryAsync<T>()`，与 async/await 无缝配合

对比方案：

| 对比项 | 裸 ADO.NET | Dapper | Entity Framework Core |
|-------|-----------|--------|---------------------|
| 代码量 | 多（每查询10-15行） | 少（每查询1-2行） | 少 |
| SQL控制 | 完全控制 | 完全控制 | 自动生成 |
| 学习成本 | 需掌握DataReader | 仅需一个Query方法 | 需学习LINQ和配置 |
| 包大小 | 无 | ~500KB | ~5MB |
| 适用项目 | 大型项目 | 中小型项目 | 大型项目 |

**结论**：对于 MoneyTracker 的项目规模和技术复杂度，Dapper 是平衡开发效率和数据库控制力的最佳选择。

#### 为什么选择 WinForm Chart 控件而非第三方图表库？

- **零依赖**：Chart 控件包含在 .NET SDK 中，无需额外安装 NuGet 包
- **功能满足**：饼图、柱状图均原生支持，标签、颜色、图例均可自定义
- **文档丰富**：微软官方文档详尽，中文教程资源多

#### 为什么选择 EPPlus 导出 Excel？

- **课程延续**：课程中已使用 Aspose.Cell 处理 Excel，EPPlus 是免费开源替代
- **功能完整**：支持单元格样式、列宽自适应、冻结窗格等
- **社区活跃**：GitHub 上持续维护，文档完善

### 3.3 第三方依赖汇总

| NuGet 包 | 用途 | 是否必需 |
|----------|------|---------|
| `System.Data.SQLite` | SQLite 数据库驱动 | ✅ |
| `Dapper` | ORM 映射 | ✅ |
| `EPPlus` | Excel 导出 | ✅ |
| `MSTest.TestFramework` | 单元测试 | 开发期 |

---

## 四、系统架构设计

### 4.1 整体架构：三层架构

```
┌─────────────────────────────────────────┐
│           Presentation 层                │
│       MoneyTracker.WinForm              │
│   (Forms / Controls / Program.cs)       │
│          ↑ 方法调用 ↓ 事件通知           │
├─────────────────────────────────────────┤
│           Business 层                    │
│         MoneyTracker.Core               │
│  (Models / Services / Interfaces)       │
│          ↑ 接口调用 ↓ 数据返回           │
├─────────────────────────────────────────┤
│            Data 层                       │
│         MoneyTracker.Data               │
│  (DataService / DatabaseInitializer)    │
│          ↑ SQL ↓ ResultSet              │
├─────────────────────────────────────────┤
│           SQLite Database               │
│          (MoneyTracker.db)              │
└─────────────────────────────────────────┘
```

### 4.2 各层职责

| 层 | 命名空间 | 主要职责 | 能否独立测试 |
|----|---------|---------|------------|
| **WinForm** | `MoneyTracker.WinForm` | 界面展示、用户交互、事件处理 | ❌ UI难自动化 |
| **Core** | `MoneyTracker.Core` | 业务规则、统计计算、预算判断、Excel导出 | ✅ 纯内存计算 |
| **Data** | `MoneyTracker.Data` | SQLite建库、CRUD操作、数据初始化 | ✅ 内存SQLite |

### 4.3 数据表设计

#### 表1：Categories（分类表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | INTEGER | PK, AUTOINCREMENT | 分类ID |
| Name | TEXT | NOT NULL | 分类名称 |
| Icon | TEXT | — | 图标名称（预留） |
| Type | INTEGER | NOT NULL | 0=支出，1=收入 |
| SortOrder | INTEGER | DEFAULT 0 | 排序序号 |
| IsDefault | INTEGER | DEFAULT 0 | 是否预设分类 |

#### 表2：Bills（账单表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | INTEGER | PK, AUTOINCREMENT | 账单ID |
| CategoryId | INTEGER | NOT NULL, FK → Categories(Id) | 分类ID |
| Amount | DECIMAL(10,2) | NOT NULL | 金额 |
| IsIncome | INTEGER | NOT NULL DEFAULT 0 | 0=支出，1=收入 |
| Date | TEXT | NOT NULL | 日期（yyyy-MM-dd） |
| Remark | TEXT | — | 备注 |
| CreatedAt | TEXT | NOT NULL | 创建时间（yyyy-MM-dd HH:mm:ss） |

#### 表3：Budgets（预算表）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | INTEGER | PK, AUTOINCREMENT | 预算ID |
| CategoryId | INTEGER | NOT NULL, FK → Categories(Id) | 分类ID |
| Month | TEXT | NOT NULL | 月份（yyyy-MM） |
| Amount | DECIMAL(10,2) | NOT NULL | 预算上限 |
| UNIQUE | (CategoryId, Month) | 联合唯一 | 每分类每月最多一条 |

### 4.4 项目目录结构

```
MoneyTracker/
│
├── MoneyTracker.sln                          # 解决方案文件
│
├── MoneyTracker.WinForm/                     # UI层
│   ├── Forms/
│   │   ├── MainForm.cs                       # 主界面（Tab导航）
│   │   ├── AddBillForm.cs                    # 添加/编辑账单
│   │   └── BudgetForm.cs                     # 预算管理
│   ├── Controls/
│   │   ├── BillListControl.cs                # 账单列表控件
│   │   ├── StatsControl.cs                   # 统计图表控件
│   │   └── BudgetControl.cs                  # 预算控件
│   └── Program.cs                            # 程序入口
│
├── MoneyTracker.Core/                        # 业务层
│   ├── Models/
│   │   ├── Bill.cs                           # 账单模型
│   │   ├── Category.cs                       # 分类模型
│   │   ├── Budget.cs                         # 预算模型
│   │   └── MonthlySummary.cs                 # 月度统计模型
│   ├── Services/
│   │   ├── BillService.cs                    # 账单业务逻辑
│   │   ├── CategoryService.cs                # 分类业务逻辑
│   │   ├── BudgetService.cs                  # 预算计算逻辑
│   │   └── ExportService.cs                  # Excel导出
│   └── Interfaces/
│       └── IDataService.cs                   # 数据访问接口
│
├── MoneyTracker.Data/                        # 数据层
│   ├── DatabaseInitializer.cs                # 建库+预设数据
│   └── DataService.cs                        # 实现IDataService
│
└── MoneyTracker.Tests/                       # 单元测试
    ├── DataServiceTests.cs                   # Data层测试（内存SQLite）
    └── BillServiceTests.cs                   # Core层测试（纯内存计算）
```

### 4.5 核心业务流程

#### 快速记账流程

```
用户点击"记账"Tab
    → 弹出 AddBillForm 窗体
    → 选择分类（ComboBox）
    → 输入金额（TextBox，数字键盘）
    → 确认日期（默认今天）
    → 可选：填写备注 / 勾选收入
    → 点击"保存"
    → DataService.AddBill()  写入 SQLite
    → 刷新账单列表 + 更新统计图表
```

#### 月度复盘流程

```
用户点击"统计"Tab
    → 显示当前月份
    → Chart 控件绘制饼图（各分类支出占比）
    → 切换月份（左右箭头）
    → DataService.GetMonthlySummary()  聚合查询
    → 图表和数据同步更新
```

### 4.6 RDD 与 TDD 的应用

#### RDD（需求驱动开发）实施路径

```
用户故事（US-01~US-08）
    → 功能拆解（MVP / V2 / V3）
    → 界面原型（底部Tab导航布局）
    → 数据表设计（3张表 + 字段约束）
    → 分层架构（3层分离）
```

每个步骤在编码开始前已经完成定义，确保开发过程有据可依，避免边做边改。

#### TDD（测试驱动开发）实施计划

**测试框架**：MSTest

**测试数据层（5个用例，内存SQLite）**：

| 编号 | 测试用例 | 验证点 |
|------|---------|--------|
| TC-01 | AddBill_增加账单后列表数量+1 | INSERT 是否生效 |
| TC-02 | GetBills_按月筛选_只返回该月数据 | WHERE Date 过滤 |
| TC-03 | DeleteBill_删除后列表少一条 | DELETE 是否生效 |
| TC-04 | UpdateBill_编辑后数据正确更新 | UPDATE 是否生效 |
| TC-05 | GetBills_按分类筛选_只返回该分类 | WHERE CategoryId 过滤 |

**测试业务层（4个用例，纯内存计算）**：

| 编号 | 测试用例 | 验证点 |
|------|---------|--------|
| TC-06 | CalculateMonthlySummary_分类汇总正确 | GROUP BY 聚合 |
| TC-07 | TotalAmount_等于各分类合计 | SUM 一致性 |
| TC-08 | IsOverBudget_超过预算返回true | 阈值判断 |
| TC-09 | IsOverBudget_未超过返回false | 阈值判断 |

**测试策略说明**：

- **Data层**：使用 `SQLiteConnection("Data Source=:memory:")` 内存模式，每个测试方法独立建库，测试结束后自动销毁。速度快、无文件残留、互不干扰
- **Core层**：纯内存计算，直接在测试中构造模型对象列表，不依赖数据库。验证的是业务逻辑正确性而非数据存取

---

## 五、开发环境说明

### 5.1 开发工具

| 工具 | 版本/说明 |
|------|----------|
| Visual Studio 2022 | 主 IDE，建议安装".NET 桌面开发"工作负载 |
| .NET SDK | .NET 8.0 |
| Git | 版本控制 |
| Windows 10/11 | 操作系统 |

### 5.2 运行环境

| 环境 | 要求 |
|------|------|
| 操作系统 | Windows 10 或 Windows 11 |
| .NET 运行时 | .NET 8.0 Desktop Runtime |
| 最低分辨率 | 1280 × 720 |
| 硬盘空间 | < 50MB（含数据库文件） |

### 5.3 NuGet 包安装命令

```bash
# 在 MoneyTracker.Data 项目中安装
dotnet add package System.Data.SQLite
dotnet add package Dapper

# 在 MoneyTracker.Core 项目中安装
dotnet add package EPPlus

# 在 MoneyTracker.Tests 项目中安装
dotnet add package MSTest.TestFramework
dotnet add package MSTest.TestAdapter
```

### 5.4 构建与运行

```bash
# 克隆项目（地址待补充）
git clone <项目地址>

# 进入项目目录
cd MoneyTracker

# 还原依赖
dotnet restore

# 运行单元测试
dotnet test

# 启动应用
dotnet run --project MoneyTracker.WinForm
```

---

## 六、项目Clone地址

>https://github.com/suifeng919/kyx

---

## 七、附录

### 7.1 术语表

| 术语 | 说明 |
|------|------|
| MVP | Minimum Viable Product，最小可行产品 |
| ORM | Object Relational Mapping，对象关系映射 |
| RDD | Requirements-Driven Development，需求驱动开发 |
| TDD | Test-Driven Development，测试驱动开发 |

### 7.2 版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| v1.0 | 2026-06-28 | 初始方案定稿 |
