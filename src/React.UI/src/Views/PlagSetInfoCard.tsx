import { Ago, AgoFormat, Card, WithIcon } from "../AzureDevOpsUI";
import { PlagiarismSet as PlagSetModel } from "../DataModels";

export function PlagSetInfoCard(props: { model: PlagSetModel | undefined | null }) : JSX.Element {
  return (
    <Card className="flex-grow"  titleProps={{ text: "Summary", ariaLevel: 3 }}>
      <div className="summary-card-content flex-grow" style={{ flexWrap: "wrap" }}>
        <div className="flex-column summary-card-content-item" style={{ flexGrow: 2 }} key={0}>
          <div className="body-m secondary-text summary-line-non-link">Create time and usage</div>
          {props.model
            ? <>
                <div className="body-m primary-text summary-line-non-link">
                  <WithIcon iconProps={{iconName: "Calendar"}}>
                    <Ago date={new Date(props.model!.create_time)} format={AgoFormat.Extended} />
                  </WithIcon>
                </div>
                <div className="body-m flex-row primary-text summary-line-non-link">
                  <WithIcon iconProps={{iconName: "Trophy2"}}>
                    <span>Contest {props.model!.related?.toLocaleString() ?? 'N/A'}</span>
                  </WithIcon>
                  <WithIcon iconProps={{iconName: "Contact"}}>
                    <span>User {props.model!.creator?.toLocaleString() ?? 'N/A'}</span>
                  </WithIcon>
                </div>
              </>
            : <>
                <div className="shimmer shimmer-line" style={{ width: '12em', margin: '3px' }}>&nbsp;</div>
                <div className="shimmer shimmer-line" style={{ width: '16em', margin: '3px' }}>&nbsp;</div>
              </>
          }
        </div>
        <div className="flex-column summary-card-content-item flex-grow" key={1}>
          <div className="body-m secondary-text summary-line-non-link">Submissions</div>
          {props.model
            ? <>
                <div className="body-m primary-text summary-line-non-link">
                  <WithIcon iconProps={{iconName: "FileCode"}}>
                    <span>{props.model!.submission_count} total</span>
                  </WithIcon>
                </div>
                <div className="body-m primary-text summary-line-non-link">
                  <WithIcon iconProps={{iconName: "Archive"}}>
                    <span>{props.model!.submission_succeeded} ok, {props.model!.submission_failed} fail</span>
                  </WithIcon>
                </div>
              </>
            : <>
                <div className="shimmer shimmer-line" style={{ width: '8em', margin: '3px' }}>&nbsp;</div>
                <div className="shimmer shimmer-line" style={{ width: '6em', margin: '3px' }}>&nbsp;</div>
              </>
          }
        </div>
        <div className="flex-column summary-card-content-item flex-grow" key={2}>
          <div className="body-m secondary-text summary-line-non-link">Reports</div>
          {props.model
            ? <>
                <div className="body-m primary-text summary-line-non-link">
                  <WithIcon iconProps={{iconName: "CRMReport"}}>
                    <span>{props.model!.report_count} total</span>
                  </WithIcon>
                </div>
                <div className="body-m primary-text summary-line-non-link">
                  <WithIcon iconProps={{iconName: "Diagnostic"}}>
                    <span>{props.model!.report_count - props.model!.report_pending} finished</span>
                  </WithIcon>
                </div>
              </>
            : <>
                <div className="shimmer shimmer-line" style={{ width: '6em', margin: '3px' }}>&nbsp;</div>
                <div className="shimmer shimmer-line" style={{ width: '8em', margin: '3px' }}>&nbsp;</div>
              </>
          }
        </div>
      </div>
    </Card>
  );
}
