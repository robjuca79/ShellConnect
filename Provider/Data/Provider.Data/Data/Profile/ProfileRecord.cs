/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Provider.Data;

/// <summary>An immutable snapshot captured when SPAD.neXt changes its active profile.</summary>
//----- TProfileRecord
public sealed record TProfileRecord
{
    #region Property
    public string? Name { get; }
    public string? Filename { get; }
    public bool IsDummyProfile { get; }
    #endregion

    #region Constructor
    public TProfileRecord (string? name, string? filename, bool isDummyProfile)
    {
        Name = name;
        Filename = filename;
        IsDummyProfile = isDummyProfile;
    }
    #endregion

    #region Members
    #endregion

    #region Fields
    #endregion

    #region Support
    #endregion

    #region Static
    #endregion
};
//---------------------------//
