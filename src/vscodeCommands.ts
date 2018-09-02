import * as vscode from "vscode";
import { getGlobalContext, runVdfIndentation } from "./common/util";
import {
  getVdfProxyService,
  setVdfProxyService,
  VdfProxyService
} from "./client/vdfProxyService";

export const formatVdfCommand = vscode.commands.registerCommand(
  "extension.FormatVDF",
  () => {
    runVdfIndentation(vscode.window.activeTextEditor.document);
  }
);

export const restartVdfServer = vscode.commands.registerCommand(
  "extension.RestartVdfServer",
  () => {
    getVdfProxyService().dispose();
  }
);
