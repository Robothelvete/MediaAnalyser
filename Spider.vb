Imports HtmlAgilityPack
Module Spider

	Sub Main()
		Dim oTCP As New Publ.TCP
		Dim oFirstpageDoc As New HtmlAgilityPack.HtmlDocument

		Dim strFirstpageHTML As String = oTCP.GetHTML("http://www.svd.se/")
		oFirstpageDoc.LoadHtml(strFirstpageHTML)

		Dim oAnchors As HtmlAgilityPack.HtmlNodeCollection = oFirstpageDoc.DocumentNode.SelectNodes("//div[@class='newsarticle']//a")

		Dim oLinks As List(Of String) = GetLinks(oAnchors)

		For Each strLink In oLinks
			'Console.WriteLine(strLink)
			Dim strHtml As String = oTCP.GetHTML(strLink)
			Dim oDoc As New HtmlDocument
			oDoc.LoadHtml(strHtml)

			Dim oArticle = GetArticle(oDoc)

			'Eh ja, sen då?
		Next

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
		Dim oArticle As New Article

		oArticle.strMetaData = oRootNode.SelectSingleNode("/p[@class='article-metadata']").InnerText
		oArticle.strRubrik = oRootNode.SelectSingleNode("/h1").InnerText
		oArticle.strIngress = oRootNode.SelectSingleNode("/p[@class='preamble']").InnerText

		Dim oNode As HtmlNode = oRootNode.SelectSingleNode("/div[@class='articlebody']/div[@class='articletext']")

		'oNode = oNode.RemoveChild(oNode.SelectSingleNode("/div[@class='article-ad']"))


		Do While oNode.SelectNodes("/blockquote").Count > 0
			oNode = oNode.RemoveChild(oNode.SelectSingleNode("/blockquote"))
		Loop

		Do While oNode.SelectNodes("/div").Count > 0
			oNode = oNode.RemoveChild(oNode.SelectSingleNode("/div"))
		Loop

		Do While oNode.SelectNodes("/h2").Count > 0
			oNode = oNode.RemoveChild(oNode.SelectSingleNode("/h2"))
		Loop

		oArticle.strArticle = oNode.InnerText

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
	End Structure

End Module
