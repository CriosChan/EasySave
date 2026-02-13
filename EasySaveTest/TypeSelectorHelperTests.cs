using EasySave.Core.Models;
using EasySave.Models.Backup;

namespace EasySaveTest;

public class TypeSelectorHelperTests
{
    [Test]
    public void GetSelector_WithCompleteBackupType_ReturnsBackupTypeComplete()
    {
        var selector = TypeSelectorHelper.GetSelector(
            BackupType.Complete, 
            "C:\\Source", 
            "C:\\Target", 
            "TestBackup");

        Assert.That(selector, Is.TypeOf<BackupTypeComplete>());
    }

    [Test]
    public void GetSelector_WithDifferentialBackupType_ReturnsBackupTypeDifferential()
    {
        var selector = TypeSelectorHelper.GetSelector(
            BackupType.Differential, 
            "C:\\Source", 
            "C:\\Target", 
            "TestBackup");

        Assert.That(selector, Is.TypeOf<BackupTypeDifferential>());
    }

    [Test]
    public void GetSelector_WithInvalidBackupType_ReturnsBackupTypeComplete()
    {
        var selector = TypeSelectorHelper.GetSelector(
            (BackupType)999, 
            "C:\\Source", 
            "C:\\Target", 
            "TestBackup");

        Assert.That(selector, Is.TypeOf<BackupTypeComplete>());
    }

    [Test]
    public void GetSelector_ReturnedSelector_IsNotNull()
    {
        var selector = TypeSelectorHelper.GetSelector(
            BackupType.Complete, 
            "C:\\Source", 
            "C:\\Target", 
            "TestBackup");

        Assert.That(selector, Is.Not.Null);
    }
}

