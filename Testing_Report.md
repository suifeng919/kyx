# MoneyTracker 测试说明文档 — TDD实践效果验证

## 一、TDD实施概述

### 1.1 开发流程

本项目遵循 **测试驱动开发（TDD）** 的红-绿-重构循环：

```
[红] 编写测试用例（定义期望行为）→ 运行测试 → 失败
[绿] 编写最少代码通过测试 → 运行测试 → 通过
[重构] 优化代码结构 → 运行测试 → 仍然通过
```

### 1.2 测试架构

```
MoneyTracker.Tests/
├── DataServiceTests.cs      # 数据层测试（5个用例）
└── BillServiceTests.cs      # 业务层测试（4个用例）
     总计：9 个测试用例
```

### 1.3 测试策略

| 测试层级 | 测试方式 | 隔离策略 | 运行速度 |
|---------|---------|---------|---------|
| **Data层** | 内存 SQLite (`:memory:`) | 每测试独立建库，互不干扰 | ~10ms/个 |
| **Core层** | 纯内存计算 | 不依赖数据库，直接构造对象 | <1ms/个 |

---

## 二、测试环境

### 2.1 运行环境

| 项目 | 说明 |
|------|------|
| .NET SDK | 8.0 |
| 测试框架 | MSTest 3.4.3 |
| 测试运行器 | dotnet test CLI / Visual Studio Test Explorer |
| 操作系统 | Windows 10+ |

### 2.2 运行测试

```bash
cd MoneyTracker
dotnet test
```

预期输出：
```
已通过! - 失败: 0，通过: 9，已跳过: 0，总计: 9，持续时间: 84 ms
```

---

## 三、测试用例详解

### 3.1 Data层测试（DataServiceTests.cs）

测试数据访问层的 CRUD 操作是否正确，使用**内存 SQLite** 确保测试的独立性和可重复性。

#### TC-01: AddBill_Should_IncreaseBillCount

| 项目 | 内容 |
|------|------|
| **测试目标** | 验证添加账单后列表中记录数+1 |
| **前置条件** | 内存数据库中已插入 3 条测试账单 |
| **测试步骤** | ① 创建新 Bill 对象 → ② 调用 AddBill() → ③ 查询 6 月份账单列表 |
| **预期结果** | 账单列表 Count == 4 |
| **TDD价值** | 确保数据写入不丢失、不重复 |

```csharp
[TestMethod]
public void AddBill_Should_IncreaseBillCount()
{
    var newBill = new Bill { CategoryId = 2, Amount = 25.00m, Date = "2024-06-15", ... };
    _service.AddBill(newBill);
    var bills = _service.GetBills(2024, 6);
    Assert.AreEqual(4, bills.Count);
}
```

#### TC-02: GetBills_ByMonth_Should_FilterCorrectly

| 项目 | 内容 |
|------|------|
| **测试目标** | 验证按月筛选只返回指定月份的数据 |
| **测试步骤** | 查询 6 月 → 查询 7 月 |
| **预期结果** | 6 月 = 3 条，7 月 = 0 条 |
| **TDD价值** | 确保 SQL 中 strftime 日期过滤逻辑正确 |

#### TC-03: DeleteBill_Should_RemoveFromDatabase

| 项目 | 内容 |
|------|------|
| **测试目标** | 验证删除后列表中少一条，且被删记录查不到 |
| **测试步骤** | ① 获取第一条账单的 Id → ② DeleteBill(id) → ③ 查询列表 → ④ 按 Id 查询 |
| **预期结果** | 列表 Count = 2，GetBillById 返回 null |
| **TDD价值** | 确保物理删除生效且级联无误 |

#### TC-04: UpdateBill_Should_ModifyExistingRecord

| 项目 | 内容 |
|------|------|
| **测试目标** | 验证编辑后金额和备注已更新 |
| **测试步骤** | ① 获取第一条账单 → ② 修改 Amount 和 Remark → ③ UpdateBill() → ④ 重新查询 |
| **预期结果** | Amount = 99.99，Remark = "修改后的备注" |
| **TDD价值** | 确保 UPDATE 语句正确修改了指定列而非全部重置 |

#### TC-05: GetBills_ByCategory_Should_FilterCorrectly

| 项目 | 内容 |
|------|------|
| **测试目标** | 验证按分类筛选|
| **测试步骤** | 按 CategoryId=1 查询 → 按 CategoryId=2 查询 |
| **预期结果** | 分类1 = 2 条（食堂），分类2 = 0 条（外卖） |
| **TDD价值** | 确保 WHERE CategoryId 过滤条件生效 |

---

### 3.2 Core层测试（BillServiceTests.cs）

测试业务逻辑的正确性，**不依赖数据库**，直接在内存中构造模型对象计算。

#### TC-06: CalculateCategoryTotal_Should_SumByCategory

| 项目 | 内容 |
|------|------|
| **测试目标** | 验证按分类汇总金额正确 |
| **测试数据** | 分类1: 15.5 + 12.0 + 8.5 = 36.0；分类2: 25.0 |
| **预期结果** | 分类1 = 36.00，分类2 = 25.00 |
| **TDD价值** | 确保聚合计算不遗漏、不多算 |

```csharp
[TestMethod]
public void CalculateCategoryTotal_Should_SumByCategory()
{
    var bills = new List<Bill>
    {
        new() { CategoryId = 1, Amount = 15.50m },
        new() { CategoryId = 1, Amount = 12.00m },
        new() { CategoryId = 2, Amount = 25.00m },
        new() { CategoryId = 1, Amount = 8.50m },
    };
    Assert.AreEqual(36.00m, _service.CalculateCategoryTotal(bills, 1));
    Assert.AreEqual(25.00m, _service.CalculateCategoryTotal(bills, 2));
}
```

#### TC-07: CalculateCategoryTotal_Should_ExcludeIncome

| 项目 | 内容 |
|------|------|
| **测试目标** | 验证支出统计不混入收入 |
| **测试数据** | 同分类3条：支出100元 + 收入500元 + 支出50元 |
| **预期结果** | 支出合计 = 150，收入合计 = 500 |
| **TDD价值** | 确保 IsIncome 过滤逻辑正确，收支分离 |

#### TC-08: CategoryTotals_Should_MatchOverallSum

| 项目 | 内容 |
|------|------|
| **测试目标** | 验证各分类合计之和等于总金额 |
| **测试数据** | 三个分类各一条：10 + 20 + 30 = 60 |
| **预期结果** | 各分类合计之和 = 总金额 = 60 |
| **TDD价值** | 确保汇总结果的一致性（数据完整性约束） |

#### TC-09: CalculateCategoryTotal_EmptyList_Should_ReturnZero

| 项目 | 内容 |
|------|------|
| **测试目标** | 验证空列表边界情况 |
| **测试数据** | 空列表 |
| **预期结果** | 返回 0 |
| **TDD价值** | 边界情况处理，防止 NullReferenceException |

---

## 四、测试覆盖分析

### 4.1 功能覆盖矩阵

| 功能模块 | 测试用例数 | 覆盖的关键路径 |
|---------|-----------|---------------|
| **账单添加** | TC-01 | INSERT 写入、SELECT 验证 |
| **月度筛选** | TC-02 | WHERE strftime 日期过滤 |
| **账单删除** | TC-03 | DELETE 物理删除、NULL 验证 |
| **账单编辑** | TC-04 | UPDATE 字段级修改 |
| **分类筛选** | TC-05 | WHERE 外键过滤 |
| **分类汇总** | TC-06、TC-07 | GROUP BY + SUM 聚合 |
| **收支分离** | TC-07 | IsIncome 布尔过滤 |
| **合计一致性** | TC-08 | 数据完整性校验 |
| **空边界** | TC-09 | 空列表容错 |

### 4.2 测试隔离性验证

```
测试方法     数据库   数据   互不干扰
─────────────────────────────────────
TC-01    :memory:   3+1条   ✅
TC-02    :memory:   3条     ✅（独立建库）
TC-03    :memory:   3条     ✅
...       ...       ...     ...
```

每个测试方法在 `[TestInitialize]` 中新建内存数据库并插入测试数据，在 `[TestCleanup]` 中释放连接。测试间完全隔离，任何顺序运行结果一致。

### 4.3 测试结果（实际运行）

```
已通过! - 失败: 0，通过: 9，已跳过: 0，总计: 9，持续时间: 84 ms
```

| 度量指标 | 数值 |
|---------|------|
| 测试总数 | 9 |
| 通过数 | 9 |
| 失败数 | 0 |
| 运行时间 | 84 ms |
| 平均每条 | ~9 ms |

---

## 五、TDD实践总结

### 5.1 收获

1. **测试即文档**：每个测试用例清晰地表达了"代码应该做什么"，比自然语言文档更精确
2. **重构安全感**：修改数据访问逻辑后运行一遍测试，9 条全部通过即可放心提交
3. **边界覆盖**：TC-09（空列表）这类边界情况，如果不先写测试很容易遗漏

### 5.2 三层架构对TDD的支撑

```
WinForm (UI)  —— ❌ 不易自动化测试
    ↓
Core (业务层) —— ✅ 纯内存计算，测试最快
    ↓
Data (数据层) —— ✅ 内存SQLite，测试隔离
    ↓
SQLite        —— 测试时替换为 :memory:
```

- **Core层**不依赖数据库，直接构造对象调用方法，测试速度 <1ms
- **Data层**通过 `:memory:` SQLite 实现完全隔离，每个测试都是独立数据库
- 如果将来要从 SQLite 换成 SQL Server，**只需重写 Data 层**，Core 层的 4 个测试完全不用改

### 5.3 改进方向

| 项目 | 现状 | 可扩展 |
|------|------|--------|
| UI测试 | 手动点击验证 | 可引入 WinAppDriver 自动化 |
| 集成测试 | 无 | 可增加端到端流程测试 |
| 覆盖率 | 9 个核心场景 | 可扩展到预算管理等更多模块 |

---

## 六、附录

### 6.1 测试文件清单

```
MoneyTracker.Tests/
├── MoneyTracker.Tests.csproj    # MSTest 项目配置
├── DataServiceTests.cs          # 数据层 5 个测试
└── BillServiceTests.cs          # 业务层 4 个测试
```

NuGet 依赖：
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.10.0" />
<PackageReference Include="MSTest.TestFramework" Version="3.4.3" />
<PackageReference Include="MSTest.TestAdapter" Version="3.4.3" />
```

### 6.2 运行命令速查

```bash
# 运行所有测试
dotnet test

# 运行指定测试类
dotnet test --filter "FullyQualifiedName~DataServiceTests"

# 运行指定测试方法
dotnet test --filter "FullyQualifiedName~AddBill_Should_IncreaseBillCount"

# 查看详细输出
dotnet test -v n
```
