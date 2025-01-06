using Mirror;
using UnityEngine;

public static class CustomReadWriteFunctions
{
    public static void WriteMigrationData(this NetworkWriter writer, MigrationData data)
    {
        writer.WriteUInt(data.OwnerNetId);
        writer.WriteString(data.ComponentName);
        writer.WriteString(data.VariableName);
        writer.WriteString(data.TypeName);
        writer.WriteString(data.SerializedValue);
    }

    public static MigrationData ReadMigrationData(this NetworkReader reader)
    {
        uint netId = reader.ReadUInt();
        string componentName = reader.ReadString();
        string variableName = reader.ReadString();
        string typeName = reader.ReadString();
        string serializedValue = reader.ReadString();

        return new MigrationData
        {
            OwnerNetId = netId,
            ComponentName = componentName,
            VariableName = variableName,
            TypeName = typeName,
            SerializedValue = serializedValue
        };
    }
}


