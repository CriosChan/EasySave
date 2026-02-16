using EasySave.Core.Models;

namespace EasySaveTest;

public class BackupTypeTests
{
    [Test]
    public void Complete_HasCorrectValue()
    {
        Assert.That((int)BackupType.Complete, Is.EqualTo(0));
    }

    [Test]
    public void Differential_HasCorrectValue()
    {
        Assert.That((int)BackupType.Differential, Is.EqualTo(1));
    }

    [Test]
    public void AllTypes_AreDefined()
    {
        var types = Enum.GetValues(typeof(BackupType));

        Assert.That(types.Length, Is.EqualTo(2));
    }

    [Test]
    public void CanAssignComplete()
    {
        BackupType type = BackupType.Complete;

        Assert.That(type, Is.EqualTo(BackupType.Complete));
    }

    [Test]
    public void CanAssignDifferential()
    {
        BackupType type = BackupType.Differential;

        Assert.That(type, Is.EqualTo(BackupType.Differential));
    }

    [Test]
    public void Complete_IsDefinedEnum()
    {
        Assert.That(Enum.IsDefined(typeof(BackupType), BackupType.Complete), Is.True);
    }

    [Test]
    public void Differential_IsDefinedEnum()
    {
        Assert.That(Enum.IsDefined(typeof(BackupType), BackupType.Differential), Is.True);
    }

    [Test]
    public void InvalidValue_IsNotDefined()
    {
        Assert.That(Enum.IsDefined(typeof(BackupType), 999), Is.False);
    }
}

