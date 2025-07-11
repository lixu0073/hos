using UnityEngine;
using System.IO;
//using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Static class providing utilities to serialize and deserialize objects in Unity Engine.
/// </summary>
public static class SaveLoadUtility
{
	private static bool isInitialized = false;
	private static void initialize()
	{
		if (isInitialized)
			throw new UnityException("SaveLoadUtility is already initialized");

		System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
		isInitialized = true;
	}

	/// <summary>
	/// Loads serialized object from file.
	/// </summary>
	/// <typeparam name="SerializedObject">Type of the object to be deserialized</typeparam>
	/// <param name="filename">The file name that the object will be deserialized from</param>
	/// <returns>Deserialized object</returns>
	public static SerializedObject LoadFromFile<SerializedObject>(string filename)
	{
		if (!isInitialized)
			initialize();

		string fullPath = Application.persistentDataPath + "/" + filename;

		if (!File.Exists(fullPath))
			throw new UnityException("File does not exist");

		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream file = File.Open(fullPath, FileMode.Open);
		SerializedObject serializedObject = (SerializedObject)binaryFormatter.Deserialize(file);
		file.Close();
		return serializedObject;
	}

	/// <summary>
	/// Serializes and saves object to file.
	/// </summary>
	/// <typeparam name="SerializedObject">Type of the object to be serialized</typeparam>
	/// <param name="serializedObject">Object to be serialized</param>
	/// <param name="filename">The file name that he object will be serialized to</param>
	public static void SaveToFile<SerializedObject>(SerializedObject serializedObject, string filename)
	{
		if (!isInitialized)
			initialize();

		BinaryFormatter binaryFormatter = new BinaryFormatter();
		try
		{
			FileStream file = File.Open(Application.persistentDataPath + "/" + filename, FileMode.OpenOrCreate);

			binaryFormatter.Serialize(file, serializedObject);
			file.Close();
		}
		catch (System.Exception e)
		{
			throw new UnityException("Error saving to file", e);
		}
	}
}