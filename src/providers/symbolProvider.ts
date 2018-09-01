import * as vscode from 'vscode';
import { CommandType, ICommand, IDefinitionResult } from '../client/proxy';
import { VdfProxyService } from '../client/vdfProxyFactory';

export class VdfDocumentSymbolProvider
  implements vscode.DocumentSymbolProvider {
  constructor(private vdfProxyService: VdfProxyService) {}
  public provideDocumentSymbols(
    document: vscode.TextDocument,
    token: vscode.CancellationToken
  ): Thenable<vscode.SymbolInformation[]> {
    const command = CommandType.Symbols;
    const fileName = document.fileName;
    const possibleWord = "";
    const lineIndex = null;
    const columnIndex = null;

    const cmd: ICommand<IDefinitionResult> = {
      command,
      possibleWord,
      fileName,
      columnIndex,
      lineIndex
    };

    return this.vdfProxyService
      .getVdfProxyHandler<IDefinitionResult>(document.uri)
      .sendCommand(cmd, token)
      .then(result => {
        console.log(result);
        return Promise.reject(null);
      });
  }
}
