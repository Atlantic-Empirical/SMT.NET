Imports System.Xml.Serialization
Imports System.IO

Namespace DotNet.Serialization

    ''' <summary>
    ''' Provides standard XML serialization operations for serializing and 
    ''' deserializing objects to files or strings.  Methods are shared and 
    ''' should be called directly.  There is no need to instantiate this 
    ''' object.
    ''' </summary>
    ''' <remarks>Use the shared methods in this object to serialize and 
    ''' deserialize objects in XML format.</remarks>
    Public Class XML

        'Constructor

        ''' <summary>
        ''' Class Serialization.XML is not intended to be instantiated.  
        ''' All methods are Shared methods and should be called directly.
        ''' </summary>
        ''' <remarks>Do not instantiate this class.</remarks>
        Private Sub New()
            'Nothing
        End Sub

        'Public Shared Methods

        ''' <summary>
        ''' Serializes a given object to the file path specified by <paramref name="aPath"/>, 
        ''' overwriting the file if it already exists.
        ''' </summary>
        ''' <param name="anObject">Object to be serialized</param>
        ''' <param name="aPath">File path to serialize to</param>
        ''' <returns><c>True</c> if successful</returns>
        ''' <remarks>Use to serialize an object to a given file path.</remarks>
        Public Shared Function SerializeToFile(ByVal anObject As Object, ByVal aPath As String) As Boolean
            Dim fs As FileStream
            Dim result As Boolean

            fs = New FileStream(aPath, FileMode.Create)
            Try
                result = SerializeToFile(anObject, fs)
            Finally
                fs.Close()
            End Try
            Return result
        End Function

        ''' <summary>
        ''' Serializes a given object to the current position in the specified file stream.
        ''' </summary>
        ''' <param name="anObject">Object to be serialized</param>
        ''' <param name="aFileStream">FileStream to serialize to</param>
        ''' <returns><c>True</c> if successful</returns>
        ''' <remarks>Use to serialize an object to a given file path.</remarks>
        Public Shared Function SerializeToFile(ByVal anObject As Object, ByVal aFileStream As FileStream) As Boolean
            Dim xs As XmlSerializer
            Dim result As Boolean

            result = False
            If anObject IsNot Nothing Then
                xs = New XmlSerializer(anObject.GetType)
                xs.Serialize(aFileStream, anObject)
                result = True
            End If
            Return result
        End Function

        ''' <summary>
        ''' Serializes a given object to a string and returns the string.
        ''' </summary>
        ''' <param name="anObject">Object to be serialized</param>
        ''' <returns>String containing the serialized object</returns>
        ''' <remarks>Use to serialize an object to a string.</remarks>
        Public Shared Function SerializeToString(ByVal anObject As Object) As String
            Dim xs As XmlSerializer
            Dim sw As StringWriter
            Dim result As String

            result = Nothing
            sw = New StringWriter
            If sw IsNot Nothing Then
                xs = New XmlSerializer(anObject.GetType)
                If xs IsNot Nothing Then
                    xs.Serialize(sw, anObject)
                    result = sw.ToString
                End If
            End If
            Return result
        End Function

        ''' <summary>
        ''' Deserializes an object of the specified type from the given file path.
        ''' </summary>
        ''' <param name="aPath">Path to existing file</param>
        ''' <param name="aType">Type of object to deserialize</param>
        ''' <returns>Object loaded from the file</returns>
        ''' <remarks>Use to deserialize an object of a known type from a given file path.</remarks>
        Public Shared Function DeserializeFromFile(ByVal aType As Type, ByVal aPath As String) As Object
            Dim fs As FileStream
            Dim result As Object

            fs = New FileStream(aPath, FileMode.Open, FileAccess.Read)
            Try
                result = DeserializeFromFile(aType, fs)
            Finally
                fs.Close()
            End Try
            Return result
        End Function

        ''' <summary>
        ''' Deserializes an object of the specified type from the current position 
        ''' in the specified file stream.
        ''' </summary>
        ''' <param name="aType">Type of object to deserialize</param>
        ''' <param name="aFileStream">Filestream from which to load object</param>
        ''' <returns>Object loaded from the file</returns>
        ''' <remarks>Use to deserialize an object of a known type from a given file stream.</remarks>
        Public Shared Function DeserializeFromFile(ByVal aType As Type, ByVal aFileStream As FileStream) As Object
            Dim xs As XmlSerializer
            Dim result As Object

            result = Nothing
            xs = New XmlSerializer(aType)
            If xs IsNot Nothing Then
                result = xs.Deserialize(aFileStream)
            End If
            Return result
        End Function

        ''' <summary>
        ''' Deserializes an object of the specified type from the given string data.
        ''' </summary>
        ''' <param name="aType">Type of objet to deserialize</param>
        ''' <param name="text">String text from which to load object</param>
        ''' <returns>Object loaded from the string</returns>
        ''' <remarks>Use to deserialize an object of a known type from a given string.</remarks>
        Public Shared Function DeserializeFromString(ByVal aType As Type, ByVal text As String) As Object
            Dim xs As XmlSerializer
            Dim sr As StringReader
            Dim result As Object

            result = Nothing
            sr = New StringReader(text)
            If sr IsNot Nothing Then
                xs = New XmlSerializer(aType)
                If xs IsNot Nothing Then
                    result = xs.Deserialize(sr)
                End If
            End If
            Return result
        End Function

        Public Shared Function SerializeToStream(ByVal anObject As Object, ByRef outStr As Stream) As Boolean
            Dim xs As XmlSerializer
            Dim result As Boolean

            result = False
            If anObject IsNot Nothing Then
                xs = New XmlSerializer(anObject.GetType)
                xs.Serialize(outStr, anObject)
                result = True
            End If
            Return result
        End Function

    End Class 'XML

    ''' <summary>
    ''' Not yet implemented.
    ''' </summary>
    ''' <remarks>Not yet implemented.</remarks>
    Public Class Binary

        'Constructor

        ''' <summary>
        ''' Class Serialization.Binary is not intended to be instantiated.  
        ''' All methods are Shared methods and should be called directly.
        ''' </summary>
        ''' <remarks>Do not instantiate this class.</remarks>
        Private Sub New()
            'Nothing
        End Sub

        'Public Shared Methods

        'ToDo: Serialization.Binary() - implement class

    End Class 'Binary

End Namespace 'Serialization