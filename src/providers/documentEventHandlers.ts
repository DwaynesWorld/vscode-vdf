import * as vscode from "vscode";
import { runVdfIndentation } from "../common/util";

export function vdfOnDidSaveTextDocument(document: vscode.TextDocument) {
  if (document.isUntitled || document.languageId !== "vdf") return;
  runVdfIndentation(document);
}
