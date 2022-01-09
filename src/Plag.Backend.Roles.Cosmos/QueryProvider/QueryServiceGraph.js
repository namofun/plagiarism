function queryServiceGraph(setId, lang, incl, excl) {
  var collection = getContext().getCollection();

  var isAccepted = collection.readDocument(
    `${collection.getAltLink()}/docs/${setId}`,
    {},
    function (err, doc, options) {
      if (err) throw err;
      if (!doc || !'data' in doc) {
        getContext().getResponse().setBody([]);
      }
      else {
        getContext().getResponse().setBody(
          Object.values(doc.data).filter(function (v) {
            return v.lang === lang && v.incl === incl && v.excl !== excl;
          })
        );
      }
    }
  );

  if (!isAccepted) {
    throw new Error('The query was not accepted by the server.');
  }
}