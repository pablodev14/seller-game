# 🪵 Seller Game · Taller de POO

<p align="center">
  <img src="https://img.shields.io/badge/Godot-4.x-478cbf?logo=godotengine&logoColor=white" alt="Godot 4">
  <img src="https://img.shields.io/badge/C%23-.NET-512BD4?logo=dotnet&logoColor=white" alt="C# y .NET">
  <img src="https://img.shields.io/badge/POO-Herencia%20%2B%20Interfaces-f59e0b" alt="Programación orientada a objetos">
  <img src="https://img.shields.io/badge/Estado-Completo-2ea44f" alt="Estado completo">
</p>

> 🎮 Videojuego 2D desarrollado en **Godot 4** con **C#**. El proyecto aplica herencia, interfaces, encapsulamiento, composición y polimorfismo a las interacciones entre un vendedor y distintos NPC.

## 🎯 Objetivo

El jugador controla a **Seller**, un vendedor que se acerca a NPC y presiona **X** para interactuar. Cada NPC expresa lo que puede hacer mediante interfaces: puede comprar madera o puede robarla. Además, Seller conserva un historial de las operaciones realizadas durante la partida.

## 🕹️ Controles

| Tecla | Acción |
| :---: | --- |
| Flechas | Mover a Seller |
| `X` | Interactuar con el NPC cercano |
| `Z` | Imprimir el historial de transacciones en la pestaña **Salida** de Godot |

> 🧠 **Ruta del refactor:** primero se eliminó la duplicación de los NPC; después se modelaron sus capacidades con interfaces; finalmente se añadieron Monk, Goblin y el historial de transacciones.

## 🧬 Paso 1 · Clase base `NPC`

La primera refactorización creó una clase base para representar el comportamiento común de los personajes no jugables:

```csharp
public partial class NPC : StaticBody2D
{
    protected AnimatedSprite2D _animator;

    public override void _Ready()
    {
        _animator = GetNode<AnimatedSprite2D>("Animator");
        _animator.Play("default");
    }
}
```

- `NPC` concentra la inicialización del sprite y la reproducción de la animación base.
- `Lancer`, `Monk` y `Goblin` heredan de `NPC`.
- Se eliminó código repetido: cada NPC dejó de implementar por separado el mismo `_Ready()` y la misma referencia a `AnimatedSprite2D`.
- El atributo `_animator` es `protected`: está encapsulado dentro de la jerarquía, pero sigue disponible para las clases hijas cuando lo necesiten.

## 🔌 Paso 2 · Interfaces de comportamiento

El segundo cambio separó la **identidad** de un NPC de sus **capacidades**:

```csharp
public interface IBuyer
{
    int Price { get; }
    bool Buy(Seller seller);
}

public interface IThief
{
    bool Steal(Seller seller);
}
```

- `IBuyer` identifica a cualquier NPC que puede comprar madera y expone su precio.
- `IThief` identifica a cualquier NPC que puede robar madera.
- Seller consulta `IBuyer` e `IThief` con *pattern matching*, en vez de preguntar si el objeto es un `Lancer`, `Monk` o `Goblin` específico.
- Gracias a este desacoplamiento, agregar un nuevo comprador o ladrón no obliga a modificar la lógica principal de Seller.

> 🔄 En el commit inicial, `Buy()` y `Steal()` no devolvían valor. En el paso 5 evolucionaron a `bool` para que Seller registre el historial únicamente cuando la operación sí tuvo éxito.

> 📌 La base de estos dos pasos se puede revisar en el commit [`refactor`](https://github.com/pablodev14/seller-game/commit/c8db7a4).

## 🧘 Paso 3 · Monk como comprador

`Monk` hereda de `NPC` e implementa `IBuyer`:

```csharp
public partial class Monk : NPC, IBuyer
{
    [Export]
    private int _price = 10;

    public int Price => _price;
}
```

- La herencia evita repetir la configuración y comportamiento común de los NPC.
- `IBuyer` representa la capacidad de comprar, no la identidad concreta de un personaje.
- El precio es privado y se expone únicamente mediante `Price`; `[Export]` permite configurarlo desde el Inspector de Godot.
- Monk tiene un precio distinto de Lancer. Al realizar una venta válida consume una madera, suma su precio a las monedas y desaparece de la escena.
- `monk.tscn` fue instanciada en `world.tscn`, por lo que participa en el mundo como los demás NPC.

## 👺 Paso 4 · Goblin como ladrón

`Goblin` hereda de `NPC` e implementa `IThief`:

```csharp
public partial class Goblin : NPC, IThief
{
    public bool Steal(Seller seller)
    {
        return seller.TryDiscountWood();
    }
}
```

La operación `TryDiscountWood()` pertenece a Seller porque Seller es dueño de su inventario y de la actualización de su HUD. Devuelve un valor booleano:

- `true`: había madera, se descuenta una unidad y el HUD se actualiza.
- `false`: la madera ya estaba en cero; no se descuenta nada ni se produce un robo.

De esta forma el inventario nunca queda negativo y Goblin no accede ni modifica directamente los atributos internos de Seller.

## 📚 Paso 5 · Historial de transacciones

Se creó la clase abstracta `Transaction`, con los datos comunes de una operación:

| Atributo | Responsabilidad |
| --- | --- |
| `NPC` | Referencia al NPC que originó la transacción. |
| `NPCName` | Nombre capturado al crear el registro; permite mostrar a Monk incluso después de `QueueFree()`. |
| `TotalCounts` | Número acumulado de operaciones para ese NPC. |

Las clases `Sale` y `Theft` heredan de `Transaction`. Así, el tipo de operación se representa con polimorfismo, no con banderas, tags o comparaciones de texto para tomar decisiones de negocio.

Seller declara una lista privada:

```csharp
private readonly List<Transaction> _transactions = new();
```

Esto es **composición**: Seller crea, guarda y controla el ciclo de vida del historial. La lista no existe de forma independiente a Seller.

Cuando se presiona `X`, Seller identifica la capacidad del NPC mediante las interfaces:

```csharp
if (npc is IBuyer buyer)
{
    if (buyer.Buy(this))
    {
        RegisterSale(npc);
    }
}
```

Antes de registrar, Seller busca si ya existe una transacción del mismo objeto `NPC`. Si existe, incrementa `TotalCounts`; si no, crea una `Sale` o `Theft`. Por ello, vender dos veces al mismo Lancer produce un único registro acumulado:

```text
Lancer | Venta | Total: 2
Goblin | Robo | Total: 2
Monk | Venta | Total: 1
```

La tecla `Z` recorre la colección e imprime el historial en **Salida** de Godot.

## ✨ Decisiones de diseño

- **Sin tipos concretos en Seller:** no existe `is Lancer`, `is Monk` ni `is Goblin`. Seller pregunta por `IBuyer` o `IThief`.
- **Encapsulamiento:** madera, monedas y contador de transacciones no pueden modificarse directamente desde otras clases. Seller ofrece métodos controlados como `TryDiscountWood()` e `IncreaseCoin(int amount)`.
- **Resultado explícito:** `Buy()` y `Steal()` devuelven `bool`. Seller registra una transacción solo cuando la operación fue exitosa.
- **Sin números mágicos en Seller:** valores como la madera inicial y la velocidad se declaran como constantes con nombre.
- **Responsabilidades separadas:** `_Process()` delega las entradas de interacción e historial a métodos privados. La lógica de cada capacidad vive en su NPC o en Seller, según corresponda.

## 🚀 Cómo ejecutar

1. Abrir el proyecto con Godot 4 instalado con soporte para C#/.NET.
2. Compilar la solución desde **Compilar → Compilar solución**.
3. Abrir `world.tscn` y ejecutar la escena con `F6`, o ejecutar el proyecto con `F5`.
4. Interactuar con los NPC y revisar el historial en la pestaña **Salida**.

## ✅ Pruebas manuales realizadas

- [x] Monk aparece en el mundo, compra madera y aplica un precio diferente al de Lancer.
- [x] Goblin descuenta madera y refleja el cambio en el HUD.
- [x] Goblin no puede robar cuando Seller tiene cero unidades de madera.
- [x] La venta y el robo crean o acumulan transacciones correctamente.
- [x] La tecla `Z` imprime el NPC, el tipo de transacción y el total acumulado.
