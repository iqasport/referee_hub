import { Datum } from "MainApp/schemas/getLanguagesSchema";

export const formatLanguage = (language: Datum) => {
  if (!language) return "---";
  const regionText = language.attributes.shortRegion ? ` - ${language.attributes.shortRegion}` : "";

  return `${language.attributes.longName}${regionText}`;
};
