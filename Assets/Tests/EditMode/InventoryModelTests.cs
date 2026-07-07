using NUnit.Framework;
using Model;
using Core;
using UnityEngine;

public class InventoryModelTests
{
    private InventoryModel _sut;
    private SimpleItemDefinition _simpleDef;
    private UniqueItemDefinition _uniqueDef;

    [SetUp]
    public void Setup()
    {
        _sut = new InventoryModel();
        _simpleDef = ScriptableObject.CreateInstance<SimpleItemDefinition>();
        _uniqueDef = ScriptableObject.CreateInstance<UniqueItemDefinition>();
    }

    [Test]
    public void StoreSimpleItem_新しいアイテムを1つ格納する_数が1になる()
    {
        _sut.StoreItem(_simpleDef, 1);
        Assert.AreEqual(1, _sut.GetStackCount(_simpleDef));
    }
    
    [Test]
    public void StoreSimpleItemMore_すでに持っているアイテムに追加で格納する_数が増える()
    {
        _sut.StoreItem(_simpleDef, 10);
        _sut.StoreItem(_simpleDef, 20);
        Assert.AreEqual(30, _sut.GetStackCount(_simpleDef));
    }

    [Test]
    public void StoreUniqueItem_新しいユニークアイテムを1つ格納する_リストを取得()
    {
        _sut.StoreItem(_uniqueDef, 1);
        Assert.AreEqual(new List<UniqueItemInstance>(new UniqueItemInstance(),new UniqueItemInstance()), _sut.GetStackCount(_uniqueDef));
    }
    

    [Test]
    public void RetrieveItem()
    {
        
    }

    [TearDown]
    public void TearDown()
    {
        _simpleDef = null;
    }
}