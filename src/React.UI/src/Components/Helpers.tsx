import { IStatusProps, Statuses } from 'azure-devops-ui/Status';
import { PlagiarismSet } from '../Models/PlagiarismSet';

export class Helpers {

  public static getStatusIndicatorData(model?: PlagiarismSet | null) : IStatusProps {
    if (model === null || model === undefined) {
      return { ...Statuses.Queued };
    } else if (model.report_count === 0 && model.submission_count === 0) {
      return { ...Statuses.Skipped, ariaLabel: "Empty" };
    } else if (model.report_pending > 0) {
      return Statuses.Running;
    } else if (model.submission_failed + model.submission_succeeded < model.submission_count) {
      return Statuses.Waiting;
    } else if (model.submission_failed > 0) {
      return Statuses.Warning;
    } else {
      return Statuses.Success;
    }
  }

  public static getPercentage(numerator: number, denominator: number) : string {
    if (numerator === 0) return '0%';
    let ratio = Math.floor(numerator * 10000 / denominator) / 100;
    return ratio.toString() + '%';
  }
}
