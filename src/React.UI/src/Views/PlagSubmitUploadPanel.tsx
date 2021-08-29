import { FormItem, ObservableValue, Panel, React, Spinner, SpinnerOrientation, TextField } from "../AzureDevOpsUI";

export interface PlagSubmitUploadPanelProps {
  busy: boolean;
  valid: boolean;
  closeUploadPanel: () => void;
  sendUploadRequest: () => void;
  language: ObservableValue<string | undefined>;
  avaliableLanguage: string[];
  exclusiveCategory: ObservableValue<string | undefined>;
  nonExclusiveCategory: ObservableValue<string | undefined>;
}

export class PlagSubmitUploadPanel extends React.Component<PlagSubmitUploadPanelProps> {

  public render() {
    return (
      <Panel
          onDismiss={this.props.closeUploadPanel}
          titleProps={{ text: "Upload submissions" }}
          footerButtonProps={[
            { text: "Cancel", onClick: this.props.closeUploadPanel, disabled: this.props.busy },
            { text: "Upload", onClick: this.props.sendUploadRequest, disabled: !this.props.valid || this.props.busy, primary: true }
          ]}>
        <form className="flex flex-column flex-grow">
          <FormItem label="Language" required>
            <TextField
              value={this.props.language}
              disabled={this.props.busy}
              onChange={(e, newValue) => (this.props.language.value = newValue)}
            />
          </FormItem>
          <FormItem label="Files" required>
            <TextField
              value={this.props.language}
              disabled={this.props.busy}
              onChange={(e, newValue) => (this.props.language.value = newValue)}
            />
          </FormItem>
          <FormItem label="Exclusive category" message="Submissions with same exclusive category will not be compared together (e.g. from same person). Leave empty if you wants to assign the submission ID so that this submission will be compared with all other submissions.">
            <TextField
              value={this.props.exclusiveCategory}
              inputType="number"
              disabled={this.props.busy}
              onChange={(e, newValue) => (this.props.exclusiveCategory.value = newValue)}
            />
          </FormItem>
          <FormItem label="Non-exclusive category" message="Submissions with different non-exclusive category will not be compared together (e.g. for same problem)." required>
            <TextField
              value={this.props.nonExclusiveCategory}
              inputType="number"
              disabled={this.props.busy}
              onChange={(e, newValue) => (this.props.nonExclusiveCategory.value = newValue)}
            />
          </FormItem>
          {this.props.busy &&
            <div style={{ margin: '24px auto 0 0' }} className="flex-row">
              <Spinner label="Uploading submissions..." orientation={SpinnerOrientation.row} />
            </div>
          }
        </form>
      </Panel>
    );
  }
}
