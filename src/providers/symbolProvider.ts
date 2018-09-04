import * as vscode from 'vscode';
import { CommandType, ICommand, IDefinitionResult } from '../client/proxy';
import { VdfProxyService } from '../client/vdfProxyService';

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

        if (result) {
          const symbols = result.definitions.map(def => {
            const uri = vscode.Uri.file(def.filePath);
            return new vscode.SymbolInformation(
              def.text,
              def.kind,
              def.container,
              new vscode.Location(
                uri,
                new vscode.Range(
                  def.range.startLine,
                  def.range.startColumn,
                  def.range.endLine,
                  def.range.endColumn
                )
              )
            );
          });

          if (symbols.length > 0) return Promise.resolve(symbols);
        }
        return Promise.reject(null);
      });
  }
}
