# 基本用法

1. 发送鼠标事件

```c#
Mouse.Event(point:Mouse.Point(100,100),
            button:Mouse.Button.Left,
            action:Mouse.Action.Click);
//button枚举值：Left，Right，Middle
//action枚举值：Click，Up，Down
```

2. 获得当前鼠标位置

```c#
Point p=Mouse.Location();
```

3. 保存当前鼠标位置

```c#
Mouse.SaveLocation();
```

4. 恢复之前保存的鼠标位置

```c#
Mouse.RestoreLocation();
```

# 备注

```c#
//Point 为 System.Drawing.Point 类
//Mouse 中实现了一个静态方法
public static Point Mouse.Point(int x,int y);
//可以返回一个 Point 类来实现更优雅的调用。
```
