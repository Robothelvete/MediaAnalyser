Imports HtmlAgilityPack
Imports System.Xml
Module Spider

	Sub Main()
		Dim oTCP As New Publ.TCP
		Dim oFirstpageDoc As New HtmlAgilityPack.HtmlDocument

		Dim strFirstpageHTML As String = oTCP.GetHTML("http://www.svd.se/")
		oFirstpageDoc.LoadHtml(strFirstpageHTML)

		Dim oAnchors As HtmlAgilityPack.HtmlNodeCollection = oFirstpageDoc.DocumentNode.SelectNodes("//div[@class='newsarticle']//a")

		Dim oLinks As List(Of String) = GetLinks(oAnchors)

		'Dim oWriter As New Xml.XmlTextWriter("C:\codeprojects\dotNet\MediaAnalysis\xml\svd\articles.xml", Nothing)
		Dim oXMLDoc As New Xml.XmlDocument
		oXMLDoc.Load("C:\codeprojects\dotNet\MediaAnalysis\xml\svd\articles.xml")


		For Each strLink In oLinks
			'Console.WriteLine(strLink)
			Dim strHtml As String = oTCP.GetHTML(strLink)
			Dim oDoc As New HtmlDocument
			oDoc.LoadHtml(strHtml)

			Dim oArticle As New Article
			oArticle = GetArticle(oDoc)
			oArticle.strURL = strLink

			oArticle.strArticle = System.Web.HttpUtility.HtmlDecode(oArticle.strArticle)

			WriteToFile(oXMLDoc, oArticle)
			oXMLDoc.Save("C:\codeprojects\dotNet\MediaAnalysis\xml\svd\articles.xml")
			System.Threading.Thread.Sleep(1500)
			'Exit For
		Next


	End Sub


	''' <summary>
	''' Lagrar den i en xmlfil
	''' </summary>
	''' <param name="oXMLDoc"></param>
	''' <param name="oArticle"></param>
	''' <remarks></remarks>
	Sub WriteToFile(ByRef oXMLDoc As Xml.XmlDocument, ByRef oArticle As Article)
		Dim oElement As XmlElement = oXMLDoc.CreateElement("article")
		Dim oIngress As XmlElement = oXMLDoc.CreateElement("ingress")
		Dim oRubrik As XmlElement = oXMLDoc.CreateElement("rubrik")
		Dim oContent As XmlElement = oXMLDoc.CreateElement("content")
		Dim oMeta As XmlElement = oXMLDoc.CreateElement("metadata")
		Dim oURL As XmlElement = oXMLDoc.CreateElement("url")

		oURL.InnerText = oArticle.strURL
		oIngress.InnerText = oArticle.strIngress
		oRubrik.InnerText = oArticle.strRubrik
		oMeta.InnerText = oArticle.strMetaData
		oContent.InnerText = oArticle.strArticle


		oElement.AppendChild(oURL)
		oElement.AppendChild(oMeta)
		oElement.AppendChild(oRubrik)
		oElement.AppendChild(oIngress)
		oElement.AppendChild(oContent)

		oXMLDoc.DocumentElement.AppendChild(oElement)

	End Sub

	''' <summary>
	''' Loopar igenom alla htmlnoder och hämtar ut en lista med unika länkar
	''' </summary>
	''' <param name="oAnchors"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Function GetLinks(ByRef oAnchors As HtmlAgilityPack.HtmlNodeCollection) As List(Of String)
		Dim oLinks As New List(Of String)
		For Each oAnchor In oAnchors
			Dim tmpString As String = oAnchor.Attributes("href").Value

			'Tvätta
			If tmpString.Contains("#") Then tmpString = tmpString.Remove(tmpString.IndexOf("#"))

			If tmpString.EndsWith(".svd") Then 'Ta bara artiklar
				If Not oLinks.Contains(tmpString) Then 'Inga dubbla entries
					oLinks.Add(tmpString)
				End If
			End If

			
		Next
		Return oLinks
	End Function

	''' <summary>
	''' Parsa och lägg in informationen på ett hanterbart sätt
	''' </summary>
	''' <param name="oDoc"></param>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Function GetArticle(ByRef oDoc As HtmlDocument) As Article
		Dim oRootNode As HtmlNode = oDoc.GetElementbyId("article-content")
		If oRootNode Is Nothing Then Return New Article
		Dim oArticle As New Article


		Try
			oArticle.strMetaData = oRootNode.SelectSingleNode("p[@class='article-metadata']").InnerText.Trim(" ", vbNewLine, vbCrLf, vbCr, vbLf)
		Catch ex As NullReferenceException
		End Try

		Try
			oArticle.strRubrik = oRootNode.SelectSingleNode("h1").InnerText.Trim(" ", vbNewLine, vbCrLf, vbCr, vbLf)
		Catch ex As NullReferenceException
		End Try

		Try
			oArticle.strIngress = oRootNode.SelectSingleNode("p[@class='preamble']").InnerText.Trim(" ", vbNewLine, vbCrLf, vbCr, vbLf)
		Catch ex As NullReferenceException
		End Try


		Dim oNode As HtmlNode = oRootNode.SelectSingleNode("div[@class='articlebody']/div[@class='articletext']")

		'oNode = oNode.RemoveChild(oNode.SelectSingleNode("/div[@class='article-ad']"))

		Try
			Do While oNode.SelectNodes("blockquote").Count > 0
				oNode.RemoveChild(oNode.SelectSingleNode("blockquote"))
			Loop

		Catch ex As NullReferenceException
		End Try

		Try
			Do While oNode.SelectNodes("div").Count > 0
				oNode.RemoveChild(oNode.SelectSingleNode("div"))
			Loop
		Catch ex As NullReferenceException
		End Try

		Try
			Do While oNode.SelectNodes("h2").Count > 0
				oNode.RemoveChild(oNode.SelectSingleNode("h2"))
			Loop
		Catch ex As NullReferenceException
		End Try

		Try
			Do While oNode.SelectNodes("comment()").Count > 0
				oNode.RemoveChild(oNode.SelectSingleNode("comment()"))
			Loop
		Catch ex As NullReferenceException
		End Try

		oArticle.strArticle = oNode.InnerText.Trim(" ", vbNewLine, vbCrLf, vbCr, vbLf)

		Return oArticle
	End Function


	''' <summary>
	''' Sätt att hantera artiklar på
	''' </summary>
	''' <remarks></remarks>
	Structure Article
		Public strArticle As String
		Public strIngress As String
		Public strRubrik As String
		Public strMetaData As String
		Public strURL As String
	End Structure

End Module
