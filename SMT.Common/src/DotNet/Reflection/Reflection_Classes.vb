Imports System.CodeDom.Compiler
Imports System.Reflection
Imports System.Text
Imports SMT.DotNet.Reflection.Enums

Namespace DotNet.Reflection

    Public Class Invoke

        ''' <summary>
        ''' Creates a new Code Provider based on the Code Provider Type specified.
        ''' </summary>
        ''' <param name="aCodeProviderType">Code Provider Type to create</param>
        ''' <returns>New CodeDomProvider instance of the type specified</returns>
        Public Shared Function GetCodeProvider(ByVal aCodeProviderType As eCodeProviderType) As CodeDomProvider
            Dim result As CodeDomProvider

            'Create Code Provider compiler interface according to type
            Select Case aCodeProviderType
                Case eCodeProviderType.CSharp
                    result = New Microsoft.CSharp.CSharpCodeProvider()
                Case eCodeProviderType.JSharp
                    ' J# requires installation of the J# Runtime Redistributable package.
                    ' For now, this will not be supported.  To add later, uncomment the
                    ' following line and remove the throw.  Also, add VJSharpCodeProvider.dll 
                    ' as a reference to the project.
                    ' --
                    'result = New Microsoft.VJSharp.VJSharpCodeProvider()
                    Throw New Exception("J# code provider type not currently supported.")
                Case Else 'eCodeProviderType.VB
                    result = New Microsoft.VisualBasic.VBCodeProvider()
            End Select
            'Return the result
            Return result
        End Function

        ''' <summary>
        ''' Creates a new CompilerParameters instance with standard settings and the 
        ''' specified references.
        ''' </summary>
        ''' <param name="References">String array of file paths to strong-named assemblies</param>
        ''' <returns>New CompilerParameters instance</returns>
        ''' <remarks>Use to get a new CompilerParameters instance with standard settings.</remarks>
        Public Shared Function GetStandardParameters(ByVal References() As String) As CompilerParameters
            Dim result As CompilerParameters
            Dim i As Integer

            result = New CompilerParameters()
            If result IsNot Nothing Then
                result.TreatWarningsAsErrors = False
                result.TempFiles.KeepFiles = False
                result.CompilerOptions = "/Optimize+ /d:TRACE=True" 'Optimize the code for faster execution, but allow tracing
                result.GenerateInMemory = True
                'Add references if specified
                If References IsNot Nothing Then
                    For i = 0 To UBound(References)
                        ' Add a reference to necessary strong-named assemblies.
                        result.ReferencedAssemblies.Add(References(i))
                    Next
                End If
            End If
            'Return the result
            Return result
        End Function

        ''' <summary>
        ''' Compiles the specified source code files using the Code Provider Type, 
        ''' References and Compiler Parameters specified, if any.
        ''' </summary>
        ''' <param name="SourceFiles">String array of file paths</param>
        ''' <param name="aCodeProviderType">Code Provider Type to use</param>
        ''' <param name="References">String array of strong-named assemblies (optional, default = <c>Nothing</c>)</param>
        ''' <param name="Params">Compiler Parameters (optional, default = <c>Nothing</c>)</param>
        ''' <returns>Compiled assembly, or Nothing if compile failed</returns>
        ''' <remarks>Use to compile a collection of source files into an assembly.</remarks>
        Public Shared Function CompileFiles(ByVal SourceFiles() As String, _
                                            Optional ByVal aCodeProviderType As eCodeProviderType = eCodeProviderType.VB, _
                                            Optional ByVal References() As String = Nothing, _
                                            Optional ByVal Params As CompilerParameters = Nothing) As Assembly
            Dim provider As CodeDomProvider
            Dim compRes As CompilerResults
            Dim msg As String
            Dim result As Assembly

            result = Nothing
            'Get Code Provider
            provider = GetCodeProvider(aCodeProviderType)
            If provider IsNot Nothing Then
                'Setup compiler parameters, if not provided
                If Params Is Nothing Then
                    Params = GetStandardParameters(References)
                End If
                'Compile script file and capture compilation results
                compRes = provider.CompileAssemblyFromFile(Params, SourceFiles)
                If compRes IsNot Nothing Then
                    ' Check whether we have errors.
                    If compRes.Errors.Count > 0 Then
                        ' Gather all error messages and display them.
                        msg = ""
                        For Each compErr As CompilerError In compRes.Errors
                            msg &= compErr.ToString & vbCr
                        Next
                        Throw New Exception("Problem compiling source files. Errors: " & vbCr & vbCr & msg)
                    Else
                        ' Compilation was successful.
                        result = compRes.CompiledAssembly
                    End If
                Else
                    Throw New Exception("Error compiling source files.")
                End If
            End If
            Return result
        End Function

        ''' <summary>
        ''' Compiles the specified source code string using the Code Provider Type, 
        ''' References and Compiler Parameters specified, if any.
        ''' </summary>
        ''' <param name="aSourceCode">String containing source code to compile</param>
        ''' <param name="aCodeProviderType">Code Provider Type to use</param>
        ''' <param name="References">String array of strong-named assemblies (optional, default = <c>Nothing</c>)</param>
        ''' <param name="Params">Compiler Parameters (optional, default = <c>Nothing</c>)</param>
        ''' <returns>Compiled assembly, or Nothing if compile failed</returns>
        ''' <remarks>Use to compile a collection of source files into an assembly.</remarks>
        ''' <exception cref="Exception">Thrown if there are errors during compile.</exception>
        Public Shared Function CompileSource(ByVal aSourceCode As String, _
                                             Optional ByVal aCodeProviderType As eCodeProviderType = eCodeProviderType.VB, _
                                             Optional ByVal References() As String = Nothing, _
                                             Optional ByVal Params As CompilerParameters = Nothing) As Assembly
            Dim provider As CodeDomProvider
            Dim compRes As CompilerResults
            Dim msg As String
            Dim result As Assembly

            result = Nothing
            'Get Code Provider
            provider = GetCodeProvider(aCodeProviderType)
            If provider IsNot Nothing Then
                'Setup compiler parameters, if not provided
                If Params Is Nothing Then
                    Params = GetStandardParameters(References)
                End If
                'Compile script file and capture compilation results
                compRes = provider.CompileAssemblyFromSource(Params, aSourceCode)
                If compRes IsNot Nothing Then
                    ' Check whether we have errors.
                    If compRes.Errors.Count > 0 Then
                        ' Gather all error messages and display them.
                        msg = ""
                        For Each compErr As CompilerError In compRes.Errors
                            msg &= compErr.ToString & vbCr
                        Next
                        Throw New Exception("Problem compiling source code. Errors: " & vbCr & vbCr & msg)
                    Else
                        ' Compilation was successful.
                        result = compRes.CompiledAssembly
                    End If
                Else
                    Throw New Exception("Error compiling source code.")
                End If
            End If
            Return result
        End Function

        ''' <summary>
        ''' Creates a new instance of the specified class in the given assembly.
        ''' </summary>
        ''' <param name="anAssembly">Assembly containing the class definition</param>
        ''' <param name="aClassName">String containing the name of the class to instantiate</param>
        ''' <returns>Returns the istantiated class object, or Nothing if none</returns>
        ''' <remarks>Use to instantiate a class in an assembly.</remarks>
        Public Shared Function GetClassInstance(ByVal anAssembly As Assembly, _
                                                ByVal aClassName As String) As Object
            Dim result As Object

            result = Nothing
            If (anAssembly IsNot Nothing) And (aClassName <> "") Then
                result = anAssembly.CreateInstance(aClassName)
            End If
            Return result
        End Function

        ''' <summary>
        ''' Creates a new instance of the first class found with the specified 
        ''' base class name in the given assembly.
        ''' </summary>
        ''' <param name="anAssembly">Assembly containing the class definition</param>
        ''' <param name="aBaseClassName">String containing the name of the base class to find</param>
        ''' <returns>Returns the istantiated class object, or Nothing if none</returns>
        ''' <remarks>Use to instantiate a class in an assembly that inherits a 
        ''' specified base class.</remarks>
        Public Shared Function GetClassInstanceByBaseName(ByVal anAssembly As Assembly, _
                                                          ByVal aBaseClassName As String) As Object
            Dim objTypes() As System.Type
            Dim objType As System.Type
            Dim result As Object

            result = Nothing
            If (anAssembly IsNot Nothing) And (aBaseClassName <> "") Then
                objTypes = anAssembly.GetTypes()
                For Each objType In objTypes
                    If objType.BaseType.Name = aBaseClassName Then
                        result = anAssembly.CreateInstance(objType.Name)
                        Exit For
                    End If
                Next
            End If
            Return result
        End Function

        ''' <summary>
        ''' Get the value of the specified property from the specified class object.
        ''' </summary>
        ''' <param name="aClassInstance">Instance of the class object containing the property</param>
        ''' <param name="aPropertyName">String containing the name of the property</param>
        ''' <param name="Index">Index of the property value to retrieve (optional, default = <c>Nothing</c>)</param>
        ''' <returns>Object representing the return value from the property, or Nothing if none</returns>
        ''' <remarks>Use to get a property value from an instantiated class.</remarks>
        Public Shared Function GetPropertyValue(ByVal aClassInstance As Object, _
                                                ByVal aPropertyName As String, _
                                                Optional ByVal Index() As Object = Nothing) As Object
            Dim classType As Type
            Dim classProperty As PropertyInfo
            Dim result As Object

            result = Nothing
            classType = aClassInstance.GetType
            If classType IsNot Nothing Then
                classProperty = classType.GetProperty(aPropertyName)
                If classProperty IsNot Nothing Then
                    result = classProperty.GetValue(aClassInstance, Index)
                End If
            End If
            Return result
        End Function

        ''' <summary>
        ''' Set the value of the specified property in the specified class object.
        ''' </summary>
        ''' <param name="aClassInstance">Instance of the class object containing the property</param>
        ''' <param name="value">Value to set</param>
        ''' <param name="aPropertyName">String containing the name of the property</param>
        ''' <param name="Index">Index of the property value to retrieve (optional, default = <c>Nothing</c>)</param>
        ''' <returns><c>True</c> if successful</returns>
        ''' <remarks>Use to set a property value in an instantiated class.</remarks>
        Public Shared Function SetPropertyValue(ByVal aClassInstance As Object, _
                                                ByVal value As Object, _
                                                ByVal aPropertyName As String, _
                                                Optional ByVal Index() As Object = Nothing) As Boolean
            Dim classType As Type
            Dim classProperty As PropertyInfo
            Dim result As Boolean

            result = False
            classType = aClassInstance.GetType
            If classType IsNot Nothing Then
                classProperty = classType.GetProperty(aPropertyName)
                If classProperty IsNot Nothing Then
                    classProperty.SetValue(aClassInstance, value, Index)
                    result = True
                End If
            End If
            Return result
        End Function

        ''' <summary>
        ''' Invoke the specified method in the specified class object.
        ''' </summary>
        ''' <param name="aClassInstance">Instance of the class object containing the property</param>
        ''' <param name="aMethodName">String containing the name of the method</param>
        ''' <param name="args">Array of arguments to the method</param>
        ''' <returns>Object containing the result of the method call, or Nothing if none</returns>
        ''' <remarks>Use to invoke a method in an instantiated class.</remarks>
        Public Shared Function ExecuteMethod(ByVal aClassInstance As Object, _
                                             ByVal aMethodName As String, _
                                             ByVal args() As Object) As Object
            Dim classType As Type
            Dim classMethod As MethodInfo
            Dim result As Object

            result = Nothing
            If aClassInstance IsNot Nothing Then
                classType = aClassInstance.GetType
                If classType IsNot Nothing Then
                    classMethod = classType.GetMethod(aMethodName)
                    If classMethod IsNot Nothing Then
                        result = classMethod.Invoke(aClassInstance, args)
                    End If
                End If
            End If
            Return result
        End Function

    End Class

End Namespace