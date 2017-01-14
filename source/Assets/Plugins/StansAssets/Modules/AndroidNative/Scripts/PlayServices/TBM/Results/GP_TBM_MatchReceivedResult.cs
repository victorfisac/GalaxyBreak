using UnityEngine;
using System.Collections;

public class GP_TBM_MatchReceivedResult  {

	private GP_TBM_Match _Match;

	public GP_TBM_MatchReceivedResult(GP_TBM_Match match) {
		_Match = match;
	} 

	public GP_TBM_Match Match {
		get {
			return _Match;
		}
	}
}
