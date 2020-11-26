
namespace HeroGame.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HeroGame.Entities;
    using HeroGame.Helpers;

    public interface IAccountService
    {
        Account Authenticate( string username, string password );
        IEnumerable<Account> GetAll();
        Account GetById( int id );
        Account Create( Account account, string password );
        void Update( Account account, string pasword = null );
        void Delete( int id );
    }

    public class AccountService : IAccountService
    {
        private DataContext _context;

        public AccountService( DataContext dataContext )
        {
            _context = dataContext;
        }

        public Account Authenticate( string username, string password )
        {
            if( string.IsNullOrEmpty( username ) || string.IsNullOrEmpty( password ) )
            {
                return null;
            }

            Account user = _context.Accounts.SingleOrDefault( x => x.UserName == username );

            // check if username exists
            if( user == null )
            {
                return null;
            }

            // check if password is correct
            if( !VerifyPasswordHash( password, user.PasswordHash, user.PasswordSalt ) )
            {
                return null;
            }

            // authentication successful
            return user;
        }

        public IEnumerable<Account> GetAll()
        {
            return _context.Accounts;
        }

        public Account GetById( int id )
        {
            return _context.Accounts.Find( id );
        }

        public Account Create( Account user, string password )
        {
            // validation
            if( string.IsNullOrWhiteSpace( password ) )
            {
                throw new AppException( "Password is required" );
            }

            if( _context.Accounts.Any( x => x.UserName == user.UserName ) )
            {
                throw new AppException( "Username \"" + user.UserName + "\" is already taken" );
            }

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash( password, out passwordHash, out passwordSalt );

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Accounts.Add( user );
            _context.SaveChanges();

            return user;
        }

        public void Update( Account userParam, string password = null )
        {
            Account account = _context.Accounts.Find( userParam.AccountId );

            if( account == null )
            {
                throw new AppException( "User not found" );
            }

            // update username if it has changed
            if( !string.IsNullOrWhiteSpace( userParam.UserName ) && userParam.UserName != account.UserName )
            {
                // throw error if the new username is already taken
                if( _context.Accounts.Any( x => x.UserName == userParam.UserName ) )
                {
                    throw new AppException( "Username " + userParam.UserName + " is already taken" );
                }

                account.UserName = userParam.UserName;
            }

            // update password if provided
            if( !string.IsNullOrWhiteSpace( password ) )
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash( password, out passwordHash, out passwordSalt );

                account.PasswordHash = passwordHash;
                account.PasswordSalt = passwordSalt;
            }

            _context.Accounts.Update( account );
            _context.SaveChanges();
        }

        public void Delete( int id )
        {
            Account account = _context.Accounts.Find( id );
            if( account != null )
            {
                _context.Accounts.Remove( account );
                _context.SaveChanges();
            }
        }

        // private helper methods

        private static void CreatePasswordHash( string password, out byte[] passwordHash, out byte[] passwordSalt )
        {
            if( password == null )
            {
                throw new ArgumentNullException( nameof( password ) );
            }

            if( string.IsNullOrWhiteSpace( password ) )
            {
                throw new ArgumentException( "Value cannot be empty or whitespace only string.", nameof( password ) );
            }

            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash( System.Text.Encoding.UTF8.GetBytes( password ) );
        }

        private static bool VerifyPasswordHash( string password, byte[] storedHash, byte[] storedSalt )
        {
            if( password == null )
            {
                throw new ArgumentNullException( nameof( password ) );
            }
            if( string.IsNullOrWhiteSpace( password ) )
            {
                throw new ArgumentException( "Value cannot be empty or whitespace only string.", nameof( password ) );
            }
            if( storedHash.Length != 64 )
            {
                throw new ArgumentException( "Invalid length of password hash (64 bytes expected).", "passwordHash" );
            }
            if( storedSalt.Length != 128 )
            {
                throw new ArgumentException( "Invalid length of password salt (128 bytes expected).", "passwordHash" );
            }

            using( var hmac = new System.Security.Cryptography.HMACSHA512( storedSalt ) )
            {
                byte[] computedHash = hmac.ComputeHash( System.Text.Encoding.UTF8.GetBytes( password ) );
                for( int i = 0; i < computedHash.Length; i++ )
                {
                    if( computedHash[i] != storedHash[i] )
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
