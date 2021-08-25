import { Ago, AgoFormat, Card, WithIcon } from "../AzureDevOpsUI";
import { PlagiarismSet as PlagSetModel } from "../DataModels";

export function PlagSetInfoCard(props: { model: PlagSetModel }) : JSX.Element {
  return (
    <Card className="flex-grow" titleProps={{ text: "Summary", ariaLevel: 3 }}>
      <div className="flex-row summary-card-content" style={{ flexWrap: "wrap" }}>
        <div className="flex-column" style={{ minWidth: "372px", width: '50%' }} key={0}>
          <div className="body-m secondary-text summary-line-non-link">Create time and usage</div>
          <div className="body-m primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "Calendar"}}>
              <Ago date={new Date(props.model.create_time)} format={AgoFormat.Extended} />
            </WithIcon>
          </div>
          <div className="body-m flex-row primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "Trophy2"}}>
              <span>Contest {props.model.related?.toLocaleString() ?? 'N/A'}</span>
            </WithIcon>
            <div style={{width: '12px'}} />
            <WithIcon iconProps={{iconName: "Contact"}}>
              <span>User {props.model.creator?.toLocaleString() ?? 'N/A'}</span>
            </WithIcon>
          </div>
        </div>
        <div className="flex-column" style={{ minWidth: "186px", width: '25%' }} key={1}>
          <div className="body-m secondary-text summary-line-non-link">Submissions</div>
          <div className="body-m primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "FileCode"}}>
              <span>{props.model.submission_count} total</span>
            </WithIcon>
          </div>
          <div className="body-m primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "Archive"}}>
              <span>{props.model.submission_succeeded} ok, {props.model.submission_failed} fail</span>
            </WithIcon>
          </div>
        </div>
        <div className="flex-column" style={{ minWidth: "186px", width: '25%' }} key={2}>
          <div className="body-m secondary-text summary-line-non-link">Reports</div>
          <div className="body-m primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "CRMReport"}}>
              <span>{props.model.report_count} total</span>
            </WithIcon>
          </div>
          <div className="body-m primary-text summary-line-non-link">
            <WithIcon iconProps={{iconName: "Diagnostic"}}>
              <span>{props.model.report_count - props.model.report_pending} finished</span>
            </WithIcon>
          </div>
        </div>
      </div>
    </Card>
  );
}
